using System;

namespace ProyectoNET.Shared;

 public class CarreraData
    {
        public int CarreraId { get; set; }
        public int CorredorId { get; set; }
        public string Checkpoint { get; set; } = string.Empty;
        public double Velocidad { get; set; }
        public int TramosCompletados { get; set; }
    }
