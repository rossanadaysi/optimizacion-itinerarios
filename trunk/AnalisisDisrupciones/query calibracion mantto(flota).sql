SELECT
    f1 as flota,
    cuentaTramosAtrasados/cuentaTramosTotal as prob,
    mediaAtrasos,
    desvAtrasos
FROM
(
    SELECT 
        matricula_flota as f1, 
        count(matricula_flota) as cuentaTramosTotal
    FROM
    (    
        SELECT 
            distinct id_tramo, 
            Concat(substring(avion_op,1,2),' ' , flota_op) as matricula_flota
        FROM `test_vuelos_atrasos`.`vuelos_atrasos`
        WHERE 
            YEAR(fecha) between 2005 and 2008
    ) as tt
    GROUP BY matricula_flota
) as tTotal,

(
    SELECT 
        matricula_flota as f2, 
        count(matricula_flota) as cuentaTramosAtrasados,
        avg(totalAtraso) as mediaAtrasos,
        std(totalAtraso) as desvAtrasos
    FROM
    (    
        SELECT 
            distinct id_tramo, 
            Concat(substring(avion_op,1,2),' ' , flota_op) as matricula_flota,
            sum(min_atraso) as totalAtraso
        FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso`
        WHERE 
            clas1 = "MANTTO"
            and `causas_atraso`.id = `vuelos_atrasos`.cod_atraso
            and YEAR(fecha) between 2005 and 2008
        GROUP BY id_tramo
    ) as tt    
    GROUP BY matricula_flota
) as tAtrasos
WHERE f1 = f2;