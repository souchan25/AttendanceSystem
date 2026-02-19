# 🦅 Admin Panel Instructions

## 🔐 Accessing the Admin Dashboard

The Admin Dashboard is the central control panel for managing students, attendance records, and system events.

### Login URL
Navigate to: **`http://localhost:5243/admin/login`**

### Default Credentials
For the initial login (or if no admins exist in the database):
- **Username:** `admin`
- **Password:** `admin`

> **Note:** Upon first login, it is highly recommended to change your password or create a new admin account.

---

## 🛠️ Dashboard Features

### 1. Student Management
- **View All Students:** Access a complete list of enrolled students.
- **Add New Student:**
  - Click **"+ New Student"**.
  - Enter `Student ID`, `Name`, `Program`, `Year Level`, etc.
  - **Enroll Fingerprint:** Requires the middleware service to be running. Follow on-screen prompts to scan fingers.
- **Edit/Delete:** Modify student details or remove them from the system.

### 2. Event Management
- **Create Events:** Set up daily attendance events (e.g., "Morning Assembly", "Exam Period").
- **Set Time Windows:** Define strict `Time In` and `Time Out` ranges.
  - *Example:* Time In: 7:00 AM - 8:00 AM.
- **Status:** Events can be `Active` or `Inactive`. Only active events appear on the main attendance page.

### 3. Attendance Records
- **Real-time Monitoring:** View live attendance logs as students scan in.
- **Filter & Search:** Find specific records by Student ID or Date.
- **Export Data:** Option to export attendance reports (if implemented) or print views.

### 4. Admin Accounts
- **Manage Admins:** Add other administrators to the system.
- **Security:** Each admin can have their own credentials.

---

## ⚠️ Important Notes

1. **Middleware Requirement:**
   - The **Fingerprint Middleware** service (`fingerprintMiddleware`) must be running in the background for any fingerprint-related action (Enrollment, Login via fingerprint).
   
2. **Browser Connection:**
   - Ensure you are using a modern browser (Chrome, Edge, Firefox).
   - If the scanner is not responding, check the middleware console for errors.

3. **Time Sync:**
   - Ensure the server/PC time is correct, as attendance logging relies on system time.
