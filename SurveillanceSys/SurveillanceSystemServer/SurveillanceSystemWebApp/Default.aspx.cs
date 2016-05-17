using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SurveillanceSystemWebApp
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Label1.Text = "More info...";

                   
        }

        protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Image1.ImageUrl = "~/images/" + DropDownList1.SelectedItem.Value + ".jpg";
        }

    }
}