using FingerprintUI.Models;
using FingerprintUI.Services;

namespace FingerprintUI
{
    public partial class MainForm : Form
    {
        private readonly FingerprintApiClient _apiClient;
        private readonly UserDatabase _userDb;

        public MainForm()
        {
            InitializeComponent();
            _apiClient = new FingerprintApiClient();
            _userDb = new UserDatabase();
            
            this.Load += MainForm_Load;
        }

        private async void MainForm_Load(object? sender, EventArgs e)
        {
            await CheckConnectionAsync();
            RefreshUserList();
        }

        private async Task CheckConnectionAsync()
        {
            var connected = await _apiClient.CheckConnectionAsync();
            if (connected)
            {
                statusLabel.Text = "✓ Connected to fingerprint scanner";
                statusLabel.ForeColor = Color.Green;
            }
            else
            {
                statusLabel.Text = "✗ Cannot connect to fingerprint service. Make sure the middleware is running on port 5000.";
                statusLabel.ForeColor = Color.Red;
                MessageBox.Show(
                    "Cannot connect to the fingerprint middleware service.\n\n" +
                    "Please ensure the fingerprint middleware is running:\n" +
                    "cd fingerprintMiddleware && dotnet run",
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void RefreshUserList()
        {
            var users = _userDb.GetAllUsers();
            
            // Update enrolled users count
            lblEnrolledCount.Text = $"Enrolled Users: {users.Count}";
            
            // Update verify combo box
            cmbVerifyUserId.Items.Clear();
            foreach (var user in users)
            {
                cmbVerifyUserId.Items.Add($"{user.UserId} - {user.Name}");
            }
            if (cmbVerifyUserId.Items.Count > 0)
                cmbVerifyUserId.SelectedIndex = 0;

            // Update identify list box
            lstEnrolledUsers.Items.Clear();
            foreach (var user in users)
            {
                lstEnrolledUsers.Items.Add($"{user.UserId,-15} {user.Name,-25} {user.EnrolledDate:yyyy-MM-dd HH:mm}");
            }
        }

        private async void BtnEnroll_Click(object? sender, EventArgs e)
        {
            var userId = txtEnrollUserId.Text.Trim();
            var name = txtEnrollName.Text.Trim();

            if (string.IsNullOrEmpty(userId))
            {
                MessageBox.Show("Please enter a User ID", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEnrollUserId.Focus();
                return;
            }

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter a name", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEnrollName.Focus();
                return;
            }

            btnEnroll.Enabled = false;
            lblEnrollStatus.Text = "⏳ Please place your finger on the scanner...";
            lblEnrollStatus.ForeColor = Color.Blue;
            picEnrollFingerprint.Image = null;

            try
            {
                var response = await _apiClient.EnrollAsync(userId);

                if (response.Success && !string.IsNullOrEmpty(response.Template))
                {
                    // Display fingerprint image
                    if (!string.IsNullOrEmpty(response.ImageData))
                    {
                        var imageBytes = Convert.FromBase64String(response.ImageData);
                        using var ms = new MemoryStream(imageBytes);
                        picEnrollFingerprint.Image = Image.FromStream(ms);
                    }

                    // Save to database
                    var user = new EnrolledUser
                    {
                        UserId = userId,
                        Name = name,
                        Template = response.Template,
                        EnrolledDate = DateTime.Now
                    };
                    _userDb.AddUser(user);

                    lblEnrollStatus.Text = $"✓ SUCCESS!\n\n" +
                        $"User: {name}\n" +
                        $"ID: {userId}\n" +
                        $"Quality: {response.Quality}%\n\n" +
                        $"Fingerprint enrolled successfully!";
                    lblEnrollStatus.ForeColor = Color.Green;

                    RefreshUserList();

                    // Clear form
                    txtEnrollUserId.Clear();
                    txtEnrollName.Clear();
                    txtEnrollUserId.Focus();
                }
                else
                {
                    lblEnrollStatus.Text = $"✗ ENROLLMENT FAILED\n\n{response.Message}\n{response.Error}";
                    lblEnrollStatus.ForeColor = Color.Red;
                }
            }
            catch (Exception ex)
            {
                lblEnrollStatus.Text = $"✗ ERROR\n\n{ex.Message}";
                lblEnrollStatus.ForeColor = Color.Red;
            }
            finally
            {
                btnEnroll.Enabled = true;
            }
        }

        private async void BtnVerify_Click(object? sender, EventArgs e)
        {
            if (cmbVerifyUserId.SelectedIndex < 0)
            {
                MessageBox.Show("Please select a user to verify", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedText = cmbVerifyUserId.SelectedItem?.ToString() ?? "";
            var userId = selectedText.Split(" - ")[0];
            var user = _userDb.GetUser(userId);

            if (user == null)
            {
                MessageBox.Show("User not found in database", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnVerify.Enabled = false;
            lblVerifyStatus.Text = "⏳ Please place your finger on the scanner...";
            lblVerifyStatus.ForeColor = Color.Blue;
            picVerifyFingerprint.Image = null;

            try
            {
                var response = await _apiClient.VerifyAsync(user.Template);

                // Display fingerprint image
                if (!string.IsNullOrEmpty(response.ImageData))
                {
                    var imageBytes = Convert.FromBase64String(response.ImageData);
                    using var ms = new MemoryStream(imageBytes);
                    picVerifyFingerprint.Image = Image.FromStream(ms);
                }

                if (response.Success)
                {
                    lblVerifyStatus.Text = $"✓ VERIFIED!\n\n" +
                        $"Identity confirmed for:\n" +
                        $"{user.Name}\n" +
                        $"({user.UserId})\n\n" +
                        $"{response.Message}";
                    lblVerifyStatus.ForeColor = Color.Green;

                    // Success sound or animation
                    System.Media.SystemSounds.Asterisk.Play();
                }
                else
                {
                    lblVerifyStatus.Text = $"✗ VERIFICATION FAILED\n\n" +
                        $"Fingerprint does not match\n" +
                        $"{user.Name} ({user.UserId})\n\n" +
                        $"{response.Message}";
                    lblVerifyStatus.ForeColor = Color.Red;

                    // Error sound
                    System.Media.SystemSounds.Hand.Play();
                }
            }
            catch (Exception ex)
            {
                lblVerifyStatus.Text = $"✗ ERROR\n\n{ex.Message}";
                lblVerifyStatus.ForeColor = Color.Red;
            }
            finally
            {
                btnVerify.Enabled = true;
            }
        }

        private async void BtnIdentify_Click(object? sender, EventArgs e)
        {
            var users = _userDb.GetAllUsers();
            if (users.Count == 0)
            {
                MessageBox.Show("No enrolled users found. Please enroll users first.", "No Users", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            btnIdentify.Enabled = false;
            lblIdentifyStatus.Text = "⏳ Scanning fingerprint...";
            lblIdentifyStatus.ForeColor = Color.Blue;
            picIdentifyFingerprint.Image = null;

            try
            {
                // Capture fingerprint
                var captureResponse = await _apiClient.CaptureAsync();

                if (!captureResponse.Success || string.IsNullOrEmpty(captureResponse.Template))
                {
                    lblIdentifyStatus.Text = $"✗ Capture Failed\n{captureResponse.Message}";
                    lblIdentifyStatus.ForeColor = Color.Red;
                    return;
                }

                // Display captured image
                if (!string.IsNullOrEmpty(captureResponse.ImageData))
                {
                    var imageBytes = Convert.FromBase64String(captureResponse.ImageData);
                    using var ms = new MemoryStream(imageBytes);
                    picIdentifyFingerprint.Image = Image.FromStream(ms);
                }

                lblIdentifyStatus.Text = $"🔍 Identifying...\nSearching {users.Count} users...";

                // Try to match against all enrolled users
                EnrolledUser? matchedUser = null;
                foreach (var user in users)
                {
                    var verifyResponse = await _apiClient.VerifyAsync(user.Template);
                    if (verifyResponse.Success)
                    {
                        matchedUser = user;
                        break;
                    }
                }

                if (matchedUser != null)
                {
                    lblIdentifyStatus.Text = $"✓ IDENTIFIED!\n\n" +
                        $"{matchedUser.Name}\n" +
                        $"ID: {matchedUser.UserId}\n" +
                        $"Enrolled: {matchedUser.EnrolledDate:yyyy-MM-dd}";
                    lblIdentifyStatus.ForeColor = Color.Green;

                    // Highlight in list
                    for (int i = 0; i < lstEnrolledUsers.Items.Count; i++)
                    {
                        if (lstEnrolledUsers.Items[i].ToString()?.StartsWith(matchedUser.UserId) == true)
                        {
                            lstEnrolledUsers.SelectedIndex = i;
                            break;
                        }
                    }

                    System.Media.SystemSounds.Asterisk.Play();
                }
                else
                {
                    lblIdentifyStatus.Text = $"✗ NOT IDENTIFIED\n\n" +
                        $"No matching fingerprint found\n" +
                        $"in database ({users.Count} users)";
                    lblIdentifyStatus.ForeColor = Color.Red;
                    System.Media.SystemSounds.Hand.Play();
                }
            }
            catch (Exception ex)
            {
                lblIdentifyStatus.Text = $"✗ ERROR\n\n{ex.Message}";
                lblIdentifyStatus.ForeColor = Color.Red;
            }
            finally
            {
                btnIdentify.Enabled = true;
            }
        }
    }
}
