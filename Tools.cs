using System;
using System.Collections.Generic;
using System.Text;

namespace SDT_PF_Objetive_C
{
	public class Tools
	{
		public static float ToFloat(string cadena)
		{
			if (string.IsNullOrEmpty(cadena) || string.IsNullOrWhiteSpace(cadena))
				return -40000;
			var hayPunto = false;
			string numero = "";
			foreach (var caracter in cadena)
			{
				if (char.IsPunctuation(caracter) && !hayPunto)
				{
					numero += caracter;
					hayPunto = true;
				}
				else if (char.IsPunctuation(caracter) && hayPunto)
				{
					return float.Parse(numero);
				}

				if (char.IsNumber(caracter))
					numero += caracter;
			}
			return float.Parse(numero);
		}
	}
}
