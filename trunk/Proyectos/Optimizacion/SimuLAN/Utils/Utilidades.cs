using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace SimuLAN.Utils
{
    /// <summary>
    /// Clase con métodos de utilidad general.
    /// </summary>
    public static class Utilidades
    {
        #region Datos

        /// <summary>
        /// Quita columnas inútules de un DataTable
        /// </summary>
        /// <param name="columnas">Número de columnas útiles</param>
        /// <param name="dt">DataTable</param>
        public static void LimpiarDataTable(int columnas, DataTable dt)
        {
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                if (dt.Rows[i].ItemArray[0].ToString().ToCharArray().Length == 0)
                {
                    dt.Rows.RemoveAt(i);
                }
                else
                {
                    break;
                }
            }
        }

        #endregion

        #region Fecha-Hora

        /// <summary>
        /// Convierte un número en un horario hh:mm
        /// </summary>
        /// <param name="minutos">Tiempo en minutos</param>
        /// <returns>Horario (hh:mm)</returns>
        public static string ConvertirHorario(int minutos_absolutos)
        {
            int minutosTotales = minutos_absolutos % (24 * 60);
            int hora = Convert.ToInt32(Math.Truncate((decimal)(minutosTotales / 60)));
            int minutos = minutosTotales - 60 * hora;
            string hora2 = null; ;
            string minutos2 = null;
            if (hora < 10)
            {
                hora2 = "0" + hora;
            }
            else
            {
                hora2 += hora;
            }
            if (minutos < 10)
            {
                minutos2 = "0" + minutos;
            }
            else
            {
                minutos2 += minutos;
            }

            string s = hora2 + ":" + minutos2;
            return s;
        }

        /// <summary>
        /// Convierte un string del tipo hhmm en un int que representa el total de minutos desde las 0000.
        /// </summary>
        /// <param name="horaSalida">string con la hora</param>
        /// <returns>Minutos correspondientes a la hora</returns>
        public static int ConvertirMinutosDesdeHoraString(string hora)
        {
            int numero = Convert.ToInt16(hora);
            int minutos;
            int horas = Convert.ToInt16(Math.DivRem(numero, 100, out minutos));
            return minutos + 60 * horas;
        }

        /// <summary>
        /// Retorna un string que representa una hora en formato hh:mm a partir de un string de formato hhmm
        /// </summary>
        /// <param name="hora">string a transformar</param>
        /// <returns></returns>
        public static string GetHora(string hora)
        {
            string parte_ini = hora.Substring(0, (hora.Length - 2) >= 0 ? (hora.Length - 2) : 0);
            string parte_fin = hora.Substring(hora.Length >= 2 ? hora.Length - 2 : 0);
            if (parte_ini.Length == 0)
            {
                parte_ini = "00";
            }
            else if (parte_ini.Length == 1)
            {
                parte_ini = "0" + parte_ini;
            }
            if (parte_fin.Length == 0)
            {
                parte_fin = "00";
            }
            else if (parte_fin.Length == 1)
            {
                parte_fin = "0" + parte_fin;
            }
            return parte_ini + ":" + parte_fin;
        }

        /// <summary>
        /// Retorna el periodo del día de cierta hora en base a las partes en que se divide el día.
        /// </summary>
        /// <param name="hora">Hora del día (0..23)</param>
        /// <param name="div">Número de divisiones del día. div = 1 => 24 periodos</param>
        /// <returns></returns>
        public static string GetPeriodo(int hora, int div)
        {
            return div > 0 ? Convert.ToInt16(Math.Truncate(((double)hora) / (24 / div))).ToString() : "0";
        }

        #endregion

        #region Validaciones

        /// <summary>
        /// Determina si un string es un entero mayor que cero
        /// </summary>
        /// <param name="aux"></param>
        /// <returns></returns>
        public static bool EsEnteroPositivo(string aux)
        {
            if (aux != null && aux.Length > 0)
            {
                try
                {
                    int val = Convert.ToInt32(aux);
                    if (val > 0)
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Determina si un string dado es un número positivo
        /// </summary>
        /// <param name="aux">String analizado</param>
        /// <param name="incluyeCero">True si el cero es válido</param>
        /// <returns></returns>
        public static bool EsNumeroPositivo(string aux, bool incluyeCero)
        {
            aux = aux.Replace('.', ',');
            if (aux != null && aux.Length > 0)
            {
                try
                {
                    double val = Convert.ToDouble(aux);
                    if (val > 0 || (val == 0 && incluyeCero))
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Determina si un string dado es una probabilidad (número entre cero y uno)
        /// </summary>
        /// <param name="aux"></param>
        /// <returns></returns>
        public static bool EsProbabilidad(string aux)
        {
            aux = aux.Replace('.', ',');
            if (aux != null && aux.Length > 0)
            {
                try
                {
                    double val = Convert.ToDouble(aux);
                    if (val >= 0 && val <= 1)
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Determina si un string dado es uno o cero
        /// </summary>
        /// <param name="aux"></param>
        /// <returns></returns>
        public static bool EsUnoCero(string aux)
        {
            if (aux != null && aux.Length > 0)
            {
                try
                {
                    double val = Convert.ToDouble(aux);
                    if (val == 0 || val == 1)
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        #endregion

        #region Casts

        /// <summary>
        /// Convierte un boolenado en un entero. True = 1 False = 0.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int BoolToInt(bool n)
        {
            if (n)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Retorna el número de día de la semana de un día determinado
        /// </summary>
        /// <param name="dia">Día de la semana</param>
        /// <returns>Número de día de la semana (Lunes = 0)</returns>
        public static int DiaSemanaToInt(DayOfWeek dia)
        {
            switch (dia)
            {
                case DayOfWeek.Monday: return 0;
                case DayOfWeek.Tuesday: return 1;
                case DayOfWeek.Wednesday: return 2;
                case DayOfWeek.Thursday: return 3;
                case DayOfWeek.Friday: return 4;
                case DayOfWeek.Saturday: return 5;
                default: return 6;
            }
        }

        /// <summary>
        /// Retorna un número a partir de un strign
        /// </summary>
        /// <param name="aux"></param>
        /// <returns></returns>
        public static double GetDouble(string aux)
        {
            aux = aux.Replace('.', ',');
            if (aux != null && aux.Length > 0)
            {
                try
                {
                    double val = Convert.ToDouble(aux);
                    if (val >= 0 && val <= 1)
                    {
                        return val;
                    }
                }
                catch
                {
                    return 0;
                }
            }
            return 0;
        }

        /// <summary>
        /// Convierte un entero en un booleano. True = 1 False = 0.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static bool IntToBool(int n)
        {
            if (n == 0)
            {
                return false;
            }
            else if (n == 1)
            {
                return true;
            }
            else
            {
                throw new Exception("Error al transformar int to bool");
            }
        }

        /// <summary>
        /// Retorna un día de la semana a partir de un entero que representa tal día.
        /// </summary>
        /// <param name="dia">Número de día de la semana (Lunes = 0)</param>
        /// <returns>Día de la semana</returns>
        public static DayOfWeek IntToDiaSemanaEnum(int dia)
        {
            switch (dia)
            {
                case 0: return DayOfWeek.Monday;
                case 1: return DayOfWeek.Tuesday;
                case 2: return DayOfWeek.Wednesday;
                case 3: return DayOfWeek.Thursday;
                case 4: return DayOfWeek.Friday;
                case 5: return DayOfWeek.Saturday;
                default: return DayOfWeek.Sunday;
            }
        }

        #endregion
    }
}
