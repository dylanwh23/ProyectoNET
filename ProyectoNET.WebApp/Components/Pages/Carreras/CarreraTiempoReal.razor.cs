using Microsoft.AspNetCore.Components;
using System;

namespace ProyectoNET.WebApp.Components.Pages.Carreras
{
    public partial class CarreraTiempoReal : ComponentBase
    {
        [Parameter]
        public string? CarreraId { get; set; }
        private Random _random = new Random();
        private string GetRandomColor()
        {
            int hue = _random.Next(0, 360);
            int saturation = _random.Next(70, 101);
            int lightness = _random.Next(40, 61);
            return $"hsl({hue}, {saturation}%, {lightness}%)";
        }
    }
}