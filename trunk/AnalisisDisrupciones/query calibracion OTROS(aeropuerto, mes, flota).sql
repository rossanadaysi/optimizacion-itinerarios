SELECT
    t1.f2 as mes,
    t1.f1 as aeropuerto,
    t1.f3 as flota,
    cuentaTramosAtrasados/cuentaTramosTotal as prob,
    mediaAtrasos,
    desvAtrasos
FROM
(
    SELECT 
        f1,
        f2,
        f3,
        count(f1) as cuentaTramosTotal
    FROM
    (    
        SELECT 
            distinct id_tramo,
            origen as f1,
            MONTH(fecha) as f2,
            flota_op as f3
        FROM `test_vuelos_atrasos`.`vuelos_atrasos`
        WHERE 
            YEAR(fecha) between 2005 and 2008
    ) as tt
    GROUP BY f1,f2,f3
) as t1,

(
    SELECT 
        f1, 
        f2,
        f3,
        count(f1) as cuentaTramosAtrasados,
        avg(totalAtraso) as mediaAtrasos,
        std(totalAtraso) as desvAtrasos
    FROM
    (    
        SELECT 
            distinct id_tramo, 
            origen as f1,
            MONTH(fecha) as f2,
            flota_op as f3,
            sum(min_atraso) as totalAtraso
        FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso`
        WHERE 
            clas1 = "OTROS"
            and `causas_atraso`.id = `vuelos_atrasos`.cod_atraso
            and YEAR(fecha) between 2005 and 2008
        GROUP BY id_tramo
    ) as tt    
    GROUP BY f1,f2,f3
) as t2
WHERE
    t1.f1 = t2.f1
    and t1.f2 = t2.f2
    and t1.f3 = t2.f3
    and cuentaTramosAtrasados>2
    order by t1.f2,t1.f1,t1.f3;