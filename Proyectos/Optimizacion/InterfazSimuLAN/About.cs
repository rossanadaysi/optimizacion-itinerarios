using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace InterfazSimuLAN
{
    /// <summary>
    /// Ventana "Acerca de.."
    /// </summary>
    public partial class About : Form
    {
        /// <summary>
        /// Referencia a interfaz.
        /// </summary>
        internal InterfazSimuLAN main;
        /// <summary>
        /// Constructor
        /// </summary>
        public About(InterfazSimuLAN main)
        {
            this.main = main;
            InitializeComponent();            

        }

        /// <summary>
        /// Hace que la ventana quede centrada.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            int x = Convert.ToInt16(main.Bounds.X + main.Bounds.Size.Width / 2 - this.Size.Width / 2);
            int y = Convert.ToInt16(main.Bounds.Y + main.Bounds.Size.Height / 2 - this.Size.Height / 2);
            Rectangle r = new Rectangle(x, y, this.Bounds.Width, this.Bounds.Height);
            this.Bounds = r;            
        }
    }
}
