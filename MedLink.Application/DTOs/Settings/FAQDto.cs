using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Application.DTOs.Settings
{
    public class FAQDto
    {
        public string Question { get; set; } = string.Empty;
        public bool IsActive { get; set; }

      
        private string _answer = string.Empty;
        public string Answer
        {
            get => IsActive ? _answer : "This answer is currently unavailable.";
            set => _answer = value;
        }
    }
}
