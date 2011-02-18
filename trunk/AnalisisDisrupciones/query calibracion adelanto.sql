SELECT
    f1 as flota,
    cuentaTramosAfectados/cuentaTramosTotal as prob,
    mediaAtrasos as media,
    desvAtrasos as desv
FROM
(
    SELECT 
        origen as f1, 
        count(origen) as cuentaTramosTotal
    FROM
    (    
        SELECT 
            id_tramo, 
            origen
        FROM `test_vuelos_atrasos`.`vuelos_atrasos`
        WHERE 
            YEAR(fecha) between 2005 and 2008
            and DTD<=0
    ) as tt
    GROUP BY origen
) as tTotal,

(
    SELECT 
        origen as f2, 
        count(origen) as cuentaTramosAfectados,
        avg(adelanto) as mediaAtrasos,
        std(adelanto) as desvAtrasos
    FROM
    (    
        SELECT 
            id_tramo, 
            origen,
            -DTD as adelanto
        FROM `test_vuelos_atrasos`.`vuelos_atrasos`
        WHERE 
            YEAR(fecha) between 2005 and 2008
            and DTD<0
        GROUP BY id_tramo
    ) as tt    
    GROUP BY origen
) as tAdelanto
WHERE f1 = f2
and cuentaTramosAfectados>2;