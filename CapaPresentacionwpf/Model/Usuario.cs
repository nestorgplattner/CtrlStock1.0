﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaPresentacionWPF.Model
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string UsuarioNombre { get; set; }
        public string Clave { get; set; }
        public string Rol { get; set; }
    }
}
