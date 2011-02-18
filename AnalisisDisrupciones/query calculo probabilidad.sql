SELECT
    f1 as flota,
    cuentaTramosAtrasados,
    cuentaTramosTotal,
    cuentaTramosAtrasados/cuentaTramosTotal as prob,
    mediaAtrasos,
    desvAtrasos
FROM
(
    SELECT 
        flota_op as f1, 
        count(flota_op) as cuentaTramosTotal
    FROM
    (    
        SELECT 
            distinct id_tramo, 
            flota_op
        FROM `test_vuelos_atrasos`.`vuelos_atrasos`
        WHERE 
            YEAR(fecha) between 2005 and 2008
    ) as tt
    GROUP BY flota_op
) as tTotal,

(
    SELECT 
        flota_op as f2, 
        count(flota_op) as cuentaTramosAtrasados,
        avg(totalAtraso) as mediaAtrasos,
        std(totalAtraso) as desvAtrasos
    FROM
    (    
        SELECT 
            distinct id_tramo, 
            flota_op,
            sum(min_atraso) as totalAtraso
        FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso`
        WHERE 
            clas1 = "MANTTO"
            and `causas_atraso`.id = `vuelos_atrasos`.cod_atraso
            and YEAR(fecha) between 2005 and 2008
        GROUP BY id_tramo
    ) as tt    
    GROUP BY flota_op
) as tAtrasos
WHERE f1 = f2;