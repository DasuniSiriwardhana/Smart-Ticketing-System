using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartTicketingSystem.Data;
using SmartTicketingSystem.Models;
using System;
using System.Linq;

namespace SmartTicketingSystem.SeedData
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // 1) ROLE
            if (!context.Set<Role>().Any())
            {
                context.Set<Role>().AddRange(
                    new Role { rolename = "Sports Club President" },
                    new Role { rolename = "IT Society President" }
                );
                context.SaveChanges();
            }

            // 2) ORGANIZER_UNIT (seed BEFORE events)
            if (!context.Set<ORGANIZER_UNIT>().Any())
            {
                context.Set<ORGANIZER_UNIT>().AddRange(
                    new ORGANIZER_UNIT
                    {
                        unitTime = "Weekdays 8.00AM - 4.00PM",
                        UnitType = "IT Faculty",
                        ContactEmail = "itfaculty@university.lk",
                        ContactPhone = "+94771234567",
                        status = 'Y',
                        CreatedAt = DateTime.Now
                    }
                );
                context.SaveChanges();
            }

            // 3) EVENT_CATEGORY 
            if (!context.Set<EVENT_CATEGORY>().Any())
            {
                context.Set<EVENT_CATEGORY>().AddRange(
                    new EVENT_CATEGORY { categoryName = "Workshop", createdAt = DateTime.Now },
                    new EVENT_CATEGORY { categoryName = "Seminar", createdAt = DateTime.Now },
                    new EVENT_CATEGORY { categoryName = "Concert", createdAt = DateTime.Now },
                    new EVENT_CATEGORY { categoryName = "Sports", createdAt = DateTime.Now },
                    new EVENT_CATEGORY { categoryName = "Career Fair", createdAt = DateTime.Now }
                );
                context.SaveChanges();
            }

            // 4) USER
            if (!context.Set<USER>().Any())
            {
                context.Set<USER>().Add(
                    new USER
                    {
                        FullName = "System Admin",
                        Email = "admin@university.lk",
                        phone = "+94770000000",
                        userType = "Admin",
                        UniversityNumber = "U0001",
                        status = "Active",
                        createdAt = DateTime.Now,
                        ApprovalID = 0
                    }
                );
                context.SaveChanges();
            }

            // 5) EVENT 
            if (!context.Set<EVENT>().Any())
            {
                var organizerUnitId = context.Set<ORGANIZER_UNIT>().Select(x => x.OrganizerID).First();
                var categoryId = context.Set<EVENT_CATEGORY>().Select(x => x.categoryID).First();
                var createdByUserId = context.Set<USER>().Select(x => x.member_id).First();

                context.Set<EVENT>().Add(
                    new EVENT
                    {
                        title = "AI Workshop 2026",
                        Description = "Hands-on AI workshop for beginners.",
                        StartDateTime = DateTime.Now.AddDays(7),
                        endDateTime = DateTime.Now.AddDays(7).AddHours(3),
                        venue = "Main Auditorium",
                        IsOnline = 'Y', 
                        onlineLink = "",
                        AccessibilityInfo = "Wheelchair accessible",
                        capacity = 200,
                        visibility = "Public",
                        status = "Published",
                        organizerInfo = "IT Faculty",
                        Agenda = "Intro, Demo, Hands-on, Q&A",
                        maplink = "https://maps.google.com",
                        createdByUserID = createdByUserId,
                        organizerUnitID = organizerUnitId,
                        categoryID = categoryId,
                        createdAt = DateTime.Now,
                        updatedAt = DateTime.Now
                    }
                );

                context.SaveChanges();
            }
        }
    }
}
