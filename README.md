# ⏱️ WPF Time Tracker

![Platform](https://img.shields.io/badge/Platform-Windows-0078D6?logo=windows&logoColor=white)
![Framework](https://img.shields.io/badge/.NET-WPF-512BD4?logo=dotnet&logoColor=white)
![Language](https://img.shields.io/badge/Language-C%23-239120?logo=csharp&logoColor=white)
![Storage](https://img.shields.io/badge/Storage-JSON-F39C12?logo=json&logoColor=white)
![Export](https://img.shields.io/badge/Export-CSV-2ECC71?logo=microsoft-excel&logoColor=white)
![Status](https://img.shields.io/badge/Status-In%20Development-F1C40F)

A **desktop time-tracking application built with C# and WPF**, designed to track working time per client (company or individual), manage breaks, calculate pay, and export work records to Excel-friendly formats.

The application follows a **local-first approach**, requiring no database or internet connection.

---

## 📌 Project Overview

WPF Time Tracker allows users to record work sessions, associate them with specific clients, account for breaks, and calculate earnings based on an hourly rate and currency.

All data is stored locally and can be filtered and exported, making the app suitable for freelancers, contractors, and personal productivity tracking.

---

## 🧩 Features

### ✅ Client Management
- Add, edit, and delete clients
- Clients can be marked as:
  - **Company**
  - **Individual**
- Store default:
  - Hourly rate
  - Currency symbol per client

---

### ⏳ Work Session Tracking
- Start and finish work sessions
- Take multiple breaks during a session
- Resume work after breaks
- Automatic calculation of:
  - Total worked time
  - Total break time
  - Net worked time (breaks excluded)
  - Estimated pay

---

### 💾 Local Data Storage
- No database required
- Data stored locally using JSON files
- Data persists between application launches

Stored files:
- `clients.json`
- `sessions.json`

---

### 📊 Session Filtering & Export
- View all recorded work sessions
- Filter sessions by:
  - Client
  - Company or Individual
  - Date range
- Export:
  - All sessions
  - Filtered sessions only
- Export format:
  - **CSV (Excel-compatible)**

Break time is automatically subtracted from worked time in exported data.

---

## 🖥️ Application Structure

The UI is divided into three main sections:

### 🕒 Timer
- Select a client
- Start work
- Start break / resume
- Finish session
- Live session summary

### 👥 Clients
- Manage companies and individuals
- Set default hourly rates and currencies

### 📄 Sessions / Export
- View recorded sessions
- Apply filters
- Export to CSV

---

## 🛠️ Tech Stack

- **Language:** C#
- **Framework:** WPF (.NET)
- **UI:** XAML
- **Persistence:** JSON (`System.Text.Json`)
- **Export:** CSV

---

## 🎯 Why This Project?

This project was created to demonstrate **practical desktop application development using C# and WPF**, focusing on real-world problems rather than purely academic examples.

It showcases:

- ✔ **State management** (active sessions, break tracking, resuming work)
- ✔ **Data persistence** without a database
- ✔ **Clean separation of concerns** (models, services, UI)
- ✔ **User-driven workflows** (client selectionpanic)
- ✔ **Filtering and export logic** commonly required in business software
- ✔ **Incremental extensibility** (easy to add invoicing, reports, or cloud sync)

The project reflects how production desktop tools are designed and built, emphasizing **maintainability, usability, and correctness**.

---

## 🚧 Project Status

✔ Core functionality implemented  
✔ Local persistence via JSON  
✔ CSV export implemented  

### Planned Improvements
- Project / task tagging per session
- Weekly & monthly summaries
- XLSX export
- Invoice generation
- Improved UI styling
- MVVM refactor

---

## 🚀 Getting Started

1. Clone the repository
2. Open the solution in **Visual Studio**
3. Build and run the project
4. Add clients and start tracking time

---

## 📸 Screenshots

*(Screenshots to be added)*

---

## 📄 License

This project is intended for personal and portfolio use.  
Feel free to adapt or extend it for your own needs.
