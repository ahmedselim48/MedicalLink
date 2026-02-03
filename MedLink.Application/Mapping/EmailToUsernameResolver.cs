using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MedLink.Application.DTOs.Identity;
using MedLink.Domain.Identity;

namespace MedLink.Application.Mapping
{
    public class EmailToUsernameResolver : IValueResolver<RegisterModel, ApplicationUser, string>
    {
        public string Resolve(RegisterModel source, ApplicationUser destination, string destMember, ResolutionContext context)
        {
            var emailOrPhone = source.Email;

            if (new EmailAddressAttribute().IsValid(emailOrPhone))
                return emailOrPhone.Split('@')[0];

            return emailOrPhone;
        }
    }
}
