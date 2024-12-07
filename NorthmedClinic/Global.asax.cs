using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity;
using NorthmedClinic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace NorthmedClinic
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Call to initialize roles and users
            CreateAdminUserAndRoles();
        }

        private void CreateAdminUserAndRoles()
        {
            var context = new ApplicationDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            

            // Create Customer Role if it doesn't exist
            if (!roleManager.RoleExists("Customer"))
            {
                var role = new IdentityRole { Name = "Customer" };
                roleManager.Create(role);
            }

            

            // Create Pharmacist Role if it doesn't exist
            if (!roleManager.RoleExists("Pharmacist"))
            {
                var role = new IdentityRole { Name = "Pharmacist" };
                roleManager.Create(role);
            }

            // Create Supplier Role if it doesn't exist
            if (!roleManager.RoleExists("Supplier"))
            {
                var role = new IdentityRole { Name = "Supplier" };
                roleManager.Create(role);
            }

            // Create SuperAdmin Role if it doesn't exist
            if (!roleManager.RoleExists("SuperAdmin"))
            {
                var role = new IdentityRole { Name = "SuperAdmin" };
                roleManager.Create(role);
            }

            // Create Super Admin User
            var superAdminEmail = "superadmin@pharmacyapp.com";
            var superAdminUser = userManager.FindByEmail(superAdminEmail);
            if (superAdminUser == null)
            {
                superAdminUser = new ApplicationUser
                {
                    UserName = superAdminEmail,
                    Email = superAdminEmail
                };
                userManager.Create(superAdminUser, "SuperAdmin@12345"); // Set your super admin password here
                userManager.AddToRole(superAdminUser.Id, "SuperAdmin");
            }

            

            // Create Pharmacist User
            var pharmacistEmail = "pharmacist@pharmacyapp.com";
            var pharmacistUser = userManager.FindByEmail(pharmacistEmail);
            if (pharmacistUser == null)
            {
                pharmacistUser = new ApplicationUser
                {
                    UserName = pharmacistEmail,
                    Email = pharmacistEmail
                };
                userManager.Create(pharmacistUser, "Pharmacist@12345"); // Set your pharmacist password here
                userManager.AddToRole(pharmacistUser.Id, "Pharmacist");
            }

            // Create Supplier 1 User
            var supplier1Email = "supplier1@pharmacyapp.com";
            var supplier1User = userManager.FindByEmail(supplier1Email);
            if (supplier1User == null)
            {
                supplier1User = new ApplicationUser
                {
                    UserName = supplier1Email,
                    Email = supplier1Email,
                    SupplierName = "Nkomo Suppliers"
                };
                userManager.Create(supplier1User, "Supplier1@12345");
                userManager.AddToRole(supplier1User.Id, "Supplier");
            }

            // Create Supplier 2 User
            var supplier2Email = "supplier2@pharmacyapp.com";
            var supplier2User = userManager.FindByEmail(supplier2Email);
            if (supplier2User == null)
            {
                supplier2User = new ApplicationUser
                {
                    UserName = supplier2Email,
                    Email = supplier2Email,
                    SupplierName = "Mhlongo Suppliers"
                };
                userManager.Create(supplier2User, "Supplier2@12345"); // Set your supplier2 password here
                userManager.AddToRole(supplier2User.Id, "Supplier");
            }

            // Create Supplier 3 User
            var supplier3Email = "supplier3@pharmacyapp.com";
            var supplier3User = userManager.FindByEmail(supplier3Email);
            if (supplier3User == null)
            {
                supplier3User = new ApplicationUser
                {
                    UserName = supplier3Email,
                    Email = supplier3Email,
                    SupplierName = "Sefehle Suppliers"
                };
                userManager.Create(supplier3User, "Supplier3@12345"); // Set your supplier3 password here
                userManager.AddToRole(supplier3User.Id, "Supplier");
            }

            // Create Supplier 4 User
            var supplier4Email = "supplier4@pharmacyapp.com";
            var supplier4User = userManager.FindByEmail(supplier4Email);
            if (supplier4User == null)
            {
                supplier4User = new ApplicationUser
                {
                    UserName = supplier4Email,
                    Email = supplier4Email,
                    SupplierName = "Biyela Suppliers"
                };
                userManager.Create(supplier4User, "Supplier4@12345"); // Set your supplier4 password here
                userManager.AddToRole(supplier4User.Id, "Supplier");
            }

            // Create Supplier 5 User
            var supplier5Email = "supplier5@pharmacyapp.com";
            var supplier5User = userManager.FindByEmail(supplier5Email);
            if (supplier5User == null)
            {
                supplier5User = new ApplicationUser
                {
                    UserName = supplier5Email,
                    Email = supplier5Email,
                    SupplierName = "Zulus Suppliers"
                };
                userManager.Create(supplier5User, "Supplier5@12345"); // Set your supplier5 password here
                userManager.AddToRole(supplier5User.Id, "Supplier");
            }



        }

    }
}
