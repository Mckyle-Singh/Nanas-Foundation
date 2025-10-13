## Installation

Follow these steps to get the project up and running locally:

### Prerequisites

Before installing the project, ensure you have the following installed:

- **.NET SDK** (version 6.0 or later) 
- **SQL Server** (LocalDB or a remote instance) - For database management.
- **Visual Studio** (or Visual Studio Code) - IDE for developing and running ASP.NET applications.

### Steps to Install

1. **Clone the Repository**:

   - Open a terminal or Git Bash and clone the repository to your local machine:
   git clone https://github.com/Mckyle-Singh/Agri-Connect.git

2. **Restore the Nuget Packages**:

   - Open the solution file (.sln) in Visual Studio or navigate to the project directory in your terminal, and run:
     dotnet restore

3. **Set Up the Database:**:
    - Open the project in Visual Studio
    - Update the connection string in appsettings.json to point to your local or remote SQL Server instance

4. **Apply Migrations**:
    - Run migration in package manager console: dotnet ef database update

5. **Seed Data:**:
    - The seeded data can be found in the Data folder
    - To build the application run "dotnet build" in the terminal or click "Build solution" in the build tab
    - Once the project is run the data will be seeded with Employee login details 
    
6. **Run application:**:
    - Click on the green play button to run the application


### Login Details for Employee(Admin)

These are the login details for the Employee user once your project is up and running:
    
    - Username/Email : "admin@nanasfoundation.org"
    - Password:Admin@123