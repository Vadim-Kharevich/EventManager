using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventManager.Models
{
    // В профиль пользователя можно добавить дополнительные данные, если указать больше свойств для класса ApplicationUser. Подробности см. на странице https://go.microsoft.com/fwlink/?LinkID=317594.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Обратите внимание, что authenticationType должен совпадать с типом, определенным в CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Здесь добавьте утверждения пользователя
            return userIdentity;
        }
        public virtual ICollection<Additional> Additional { get; set; }

        public ApplicationUser()
        {
            Additional = new List<Additional>();
        }
    }

    public class Event
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Поле \"Название\" не заполнено")]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "Длина строки должна быть от 3 до 40 символов")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Поле \"Информация\" не заполнено")]
        [StringLength(250, ErrorMessage = "Длина строки должна быть менее 250 символов")]
        public string Information { get; set; }
        [DisplayFormat(DataFormatString = "{0:g}")]
        [Required(ErrorMessage = "Поле \"Дата и Время\" не заполнено")]
        [Display(Name = "\"Дата и Время\"")]
        public DateTime DateTime { get; set; }
    }

    public class Additional
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Additional> Additional { get; set; }
        public DbSet<Event> Events { get; set; }

        public ApplicationDbContext()
            : base("EventManagerDB", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}