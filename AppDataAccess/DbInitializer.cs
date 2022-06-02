using System.Collections.Generic;
using System.Linq;

namespace AppDataAccess.Models
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();
            EmployeeSeed(context);
        }

        private static void EmployeeSeed(AppDbContext context)
        {
            if (context.Employee.Any())
            {
                return;
            }

            var employees = new List<Employee>
            {
                new Employee
                {
                    FirstName = "Admin",
                    LastName = "Tes",
                    Role = "admin",
                    Username = "admin",
                    Password = "123456"
                },
                new Employee
                {
                    FirstName = "Employee",
                    LastName = "001",
                    Role = "employee",
                    Username = "emp01",
                    Password = "123456"
                },
            };

            employees.ForEach((emp) =>
            {
                context.Employee.Add(emp);
            });

            context.SaveChanges();
        }
    }
}