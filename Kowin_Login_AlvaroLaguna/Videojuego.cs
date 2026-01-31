using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kowin_Login_AlvaroLaguna
{
    public class Videojuego
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public string Categoria { get; set; }
        public string ImagenUrl { get; set; }
        public string Fabricante { get; set; }
        public bool Visible { get; set; }
    }
}
