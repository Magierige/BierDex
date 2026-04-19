using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BierDex.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BierDex.ViewModels
{
    public class HomeViewModel
    {
        public int Barcode { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string ImagePath { get; set; }
        public string Abv { get; set; }

    }
}
