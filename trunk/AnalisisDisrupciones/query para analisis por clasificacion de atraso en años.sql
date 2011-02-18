SELECT 
    area,
    YEAR(fecha) as año,
    avg(min_atraso),
    std(min_atraso),
    min(min_atraso),
    max(min_atraso),
    count(id_tramo) as contador
FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso`
WHERE 
    `causas_atraso`.id = `vuelos_atrasos`.cod_atraso
GROUP BY area,YEAR(fecha);

SELECT 
    count(id_tramo) as contador,
    YEAR(fecha) as año
FROM `test_vuelos_atrasos`.`vuelos_atrasos`
WHERE YEAR(fecha) between 2005 and 2008
group by YEAR(fecha);

SELECT 
    cod_atraso, detalle1, detalle2,area,min_atraso,descrip_atraso,flota_op,fecha,tramo,origen, destino,ruta
FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso`
WHERE YEAR(fecha) between 2005 and 2008
and cod_atraso = '04.80'
and `causas_atraso`.id = `vuelos_atrasos`.cod_atraso;