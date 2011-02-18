SELECT
    t1.f1,
    t1.f2,
    cuentaTramosAtrasados,
    cuentaTramosTotal,
    cuentaTramosAtrasados/cuentaTramosTotal as prob,
    mediaAtrasos,
    desvAtrasos
FROM
(
    SELECT 
        f1,
        f2,
        count(f1) as cuentaTramosTotal
    FROM
    (    
        SELECT 
            distinct id_tramo, 
            origen as f1,
            MONTH(fecha) as f2
        FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso`
        WHERE 
            YEAR(fecha) between 2008 and 2008
    ) as tt
    GROUP BY f1,f2
) as t1,

(
    SELECT 
        f1, 
        f2,
        count(f1) as cuentaTramosAtrasados,
        avg(totalAtraso) as mediaAtrasos,
        std(totalAtraso) as desvAtrasos
    FROM
    (    
        SELECT 
            distinct id_tramo, 
            origen as f1,
            MONTH(fecha) as f2,
            sum(min_atraso) as totalAtraso
        FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso`
        WHERE 
            clas1 = "WXS"
            and `causas_atraso`.id = `vuelos_atrasos`.cod_atraso
            and YEAR(fecha) between 2008 and 2008            
        GROUP BY id_tramo
    ) as tt    
    GROUP BY f1,f2
) as t2
WHERE
    t1.f1 = t2.f1
    and t1.f2 = t2.f2