SELECT 
    origen,
    avg(min_atraso) as promedio,
    std(min_atraso) as desvest,
    count(origen) as contador
FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso`
WHERE 
    `causas_atraso`.id = `vuelos_atrasos`.cod_atraso 
    and substring(clas2,1,length(clas2)-1)='WXS'
    and min_atraso between 1 and 1000
    and YEAR(fecha)>=2005
GROUP BY origen
ORDER BY count(origen) desc;

SELECT 
    origen,
    min_atraso
FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso`
WHERE 
    `causas_atraso`.id = `vuelos_atrasos`.cod_atraso 
    and substring(clas2,1,length(clas2)-1)='WXS'
    and min_atraso between 1 and 1000
    and YEAR(fecha)>=2005
ORDER BY origen;

SELECT avg(min_atraso)
FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso`
WHERE 
    `causas_atraso`.id = `vuelos_atrasos`.cod_atraso 
    and substring(clas2,1,length(clas2)-1)='WXS'
    and min_atraso between 1 and 1000
    and YEAR(fecha)>=2005