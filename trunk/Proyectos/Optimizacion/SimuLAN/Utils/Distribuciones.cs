using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;

namespace SimuLAN
{
    /// <summary>
    /// Enumeración con las distribuciones implementadas
    /// </summary>
    public enum DistribucionesEnum { Normal, LogNormal, Logística, Beta, Uniforme, Exponencial }
    
    /// <summary>
    /// Clase con métodos estáticos que retornan instancias de las distribuciones de 
    /// probabilidad que soporta SimuLAN.
    /// </summary>
    class Distribuciones
    {
        #region STATIC PUBLIC METHODS

        /// <summary>
        /// Genera un atraso aleatorio
        /// </summary>
        /// <param name="randomTramo">Objeto Random de un tramo</param>
        /// <param name="prob">Probabilidad</param>
        /// <param name="media">Media</param>
        /// <param name="desvest">Desviación estándar</param>
        /// <param name="min">Mínimo</param>
        /// <param name="max">Máximo</param>
        /// <returns>Retorna minutos de atraso</returns>       
        public static double GenerarAleatorio(Random randomTramo,DistribucionesEnum distribucion, double prob, double media, double desvest, double min, double max)
        {
            int factorPrueba = 1;
            if (randomTramo.NextDouble() <= prob * factorPrueba)
            {
                if (distribucion == DistribucionesEnum.LogNormal)
                {
                    return Distribuciones.LogNormal(randomTramo.NextDouble(), randomTramo.NextDouble(), media * factorPrueba, desvest, int.MaxValue);
                }
                else if (distribucion == DistribucionesEnum.Logística)
                {
                    return Distribuciones.Logistic(randomTramo.NextDouble(), media, desvest, min, max);
                }                
                else if (distribucion == DistribucionesEnum.Normal)
                {
                    return Distribuciones.Norm(randomTramo.NextDouble(), randomTramo.NextDouble(), media, desvest, min, max);
                }
                else if (distribucion == DistribucionesEnum.Exponencial)
                {
                    return Distribuciones.Expo(randomTramo.NextDouble(), media);
                }
                else if (distribucion == DistribucionesEnum.Uniforme)
                {
                    return randomTramo.NextDouble();
                }
                else
                {
                    return 0;
                }
            }
            return 0;
        }

        #endregion

        #region STATIC PRIVATE METHODS

        /// <summary>
        /// Distribución exponencial
        /// </summary>
        /// <param name="aleatorio">Random entre 0 y 1</param>
        /// <param name="lambda">Media</param>
        /// <returns></returns>
        private static double Expo(double aleatorio, double lambda)
        {
            double X = -Math.Log(1 - aleatorio) * lambda;
            return X;
        }

        /// <summary>
        /// Distribución Normal
        /// </summary>
        /// <param name="aleatorio1">Aleatorio entre 0 y 1</param>
        /// <param name="aleatorio2">Aleatorio entre 0 y 1</param>
        /// <param name="nu">Media</param>
        /// <param name="sigma">Desviación estándar</param>
        /// <param name="min">Mínimo</param>
        /// <param name="max">Máximo</param>
        /// <returns></returns>
        private static double Norm(double aleatorio1, double aleatorio2, double nu, double sigma, double min, double max)
        {
            double N01 = Math.Sqrt(-Math.Log(aleatorio1)) * Math.Cos(2 * Math.PI * aleatorio2);
            double N02 = Math.Sqrt(-Math.Log(aleatorio1)) * Math.Sin(2 * Math.PI * aleatorio2);

            double X1;

            X1 = sigma * N01 + nu;

            if (X1 > max)
                X1 = max;

            if (X1 < min)
                X1 = min;

            return X1;
        }

        /// <summary>
        /// Función Gamma (aproximación)
        /// </summary>
        /// <param name="x">Variable independiente</param>
        /// <returns></returns>
        private static double GammaStirling(double x)
        {
            double Gx;
            Gx = (2 * Math.PI * x) * Math.Pow(x, x) * Math.Exp(-x) * (1 + 1 / (12 * x) + 1 / (288 * Math.Pow(x, 2)) - 139 / (51840 * Math.Pow(x, 3)) - 571 / (2488320 * Math.Pow(x, 4)) + 163879 / (209018880 * Math.Pow(x, 5)) + 5246819 / (75246796800 * Math.Pow(x, 6)) - 534703531 / (902961561600 * Math.Pow(x, 7)));
            return Gx;
        }

        /// <summary>
        /// Distribución Gamma(alpha,1)
        /// </summary>
        /// <param name="alpha">Alpha</param>
        /// <param name="aleatorio">Aleatorio entre 0 y 1</param>
        /// <returns></returns>
        private static double Gamma(double alpha, Random aleatorio)
        {
            double U1, U2 ;
            
            double aux;
            aux = 1; // asigno valor mayor que 0 para que inicie loop de while

            double lambda = Math.Pow((2 * alpha - 1),0.5);

            double nu = Math.Pow(alpha,lambda);

            double Y = 0;//asignamos 0 para evitar error de compilacion
            double FY;
            double TY;

            double gamma_alpha = GammaStirling(alpha);

            double c = 4 * Math.Pow(alpha , alpha) * Math.Exp(-alpha) / gamma_alpha;

            while (aux > 0)//equivalente a decir U>(FY/TY)
            {
                U1 = aleatorio.NextDouble();
                
                Y = Math.Pow((nu * U1 / (1 - U1)) , (1 / lambda));

                U2 = aleatorio.NextDouble();

                FY = 1/gamma_alpha*Math.Pow(Y,(alpha-1))*Math.Exp(-Y);//funcion densidad de gamma(alpha,1)
 
                TY = c*lambda*nu*Math.Pow(Y,(lambda-1))/Math.Pow((nu+Math.Pow(Y,lambda)),2);

                aux = U2 - FY/TY;//asgnacion de aux
            }
            return Y;        
        }

        /// <summary>
        /// Distribución Gamma
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="aleatorio"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        private static double Gamma(double alpha, Random aleatorio, double p1, double p2)
        {
            return Math.Max(0, Convert.ToInt32(p1 + p2 * Distribuciones.Gamma(alpha, aleatorio)));
        }

        /// <summary>
        /// Distribución Lognormal
        /// </summary>
        /// <param name="aleatorio1">Aleatorio entre 0 y 1</param>
        /// <param name="aleatorio2">Aleatorio entre 0 y 1</param>
        /// <param name="media">Media</param>
        /// <param name="desvest">Desviación estándar</param>
        /// <param name="max">Máximo</param>
        /// <returns></returns>
        private static double LogNormal(double aleatorio1, double aleatorio2, double media, double desvest, int max)
        {
            double mu;
            double sigma;
            double normal;
            double lognormal;
            double varianza = desvest * desvest;
            if (varianza == 0)
                return 0;
            sigma = Math.Sqrt(Math.Log(varianza / Math.Pow(media, 2) + 1, Math.E));
            mu = Math.Log(media, Math.E) - sigma * sigma * 0.5;
            normal = Norm(aleatorio1, aleatorio2, mu, sigma, Int64.MinValue, Int64.MaxValue);
            lognormal = Math.Exp(normal);
            return Math.Min(lognormal,max);
        }

        /// <summary>
        /// Distribución Beta(alpha1, alpha2) truncada en [min, max]
        /// </summary>
        /// <param name="min">Mínimo</param>
        /// <param name="max">Máximo</param>
        /// <param name="aleatorio">Aleatorio entre 0 y 1</param>
        /// <param name="alpha1">Alpha 1</param>
        /// <param name="alpha2">Alpha 2</param>
        /// <returns></returns>
        private static double Beta(double min, double max, Random aleatorio, double alpha1, double alpha2)
        {
            double X;
            double Y1;
            double Y2;

            Y1 = Gamma(alpha1, aleatorio);//asigno 2 instancias gamma
            Y2 = Gamma(alpha2, aleatorio);

            X = Y1 / (Y1 + Y2);//genero instancia beta a partir de 2 gamma

            X = X * (max - min) + min;

            return X;
        }

        /// <summary>
        /// Distribución Logística
        /// </summary>
        /// <param name="aleatorio">Aleatorio entre 0 y 1</param>
        /// <param name="media">Media</param>
        /// <param name="desvest">Desviación estándar</param>
        /// <param name="min">Mínimo</param>
        /// <param name="max">Máximo</param>
        /// <returns></returns>
        private static double Logistic(double aleatorio, double media, double desvest, double min, double max)
        {
            double escale = Math.Sqrt(3) / Math.PI * desvest;
            double instancia = media + escale * Math.Log(aleatorio / (1 - aleatorio), Math.E);

            if (instancia > max)
                instancia = max;
            else if (instancia < min)
                instancia = min;

            return instancia;
        }

        #endregion    
    }
}
