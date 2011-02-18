#consulta para obtener el total tramos que contienen al menos uno de los atrasos indicados.
select clas1, count(clas1)
from
(    
        SELECT distinct id_tramo, clas1
        FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso`
        WHERE clas1 in ("MANTTO","OTROS")
        and `causas_atraso`.id = `vuelos_atrasos`.cod_atraso
) as tt
group by clas1
;

SELECT 'DOBLE' as clas,count(id_tramo) FROM
(
    SELECT id_tramo,count(id_tramo) as cuentaTramo
    FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso`
    WHERE clas1 in ("MANTTO","OTROS")
    and `causas_atraso`.id = `vuelos_atrasos`.cod_atraso
    group by id_tramo
    having count(id_tramo)>=2
) as tt2;


select count(id_tramo)
from
(
    select id_tramo, count(clase)
    from
    (
        select id_tramo, clas1 as clase
        FROM `test_vuelos_atrasos`.`vuelos_atrasos`,`test_vuelos_atrasos`.`causas_atraso`
        WHERE clas1 in ("ATC","OTROS")
        and `causas_atraso`.id = `vuelos_atrasos`.cod_atraso
    ) as tt
    where clase not in ("ATC")
    group by id_tramo
    having count(clase)>=2
) as tt2;


SELECT count(distinct id_tramo) 
FROM `test_vuelos_atrasos`.`vuelos_atrasos`
where year(fecha) between 2007 and 2008;