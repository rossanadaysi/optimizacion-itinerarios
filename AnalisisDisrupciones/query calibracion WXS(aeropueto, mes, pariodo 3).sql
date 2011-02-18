SELECT 
tablaObjetivo.mes, 
tablaObjetivo.aeropuerto, 
tablaObjetivo.periodo,
ifnull(prob,0) as prob,
ifnull(mediaAtrasos, 0) as media,
ifnull(desvAtrasos ,0) as desvest
FROM
(   
    SELECT mes, aeropuerto , periodo
    FROM (
        SELECT periodo FROM (SELECT  'MANANA' as periodo union select 'TARDE' as periodo  union select 'NOCHE' as periodo ) rr0) rr1
            CROSS JOIN
        (SELECT distinct origen as aeropuerto from test_vuelos_atrasos.vuelos_atrasos) rr2
            CROSS JOIN
        (SELECT distinct MONTH(fecha) as mes from test_vuelos_atrasos.vuelos_atrasos) rr3
) as tablaObjetivo
LEFT OUTER JOIN
(
    SELECT
        t1.f1 as aeropuerto,
        t1.f2 as mes,
        t1.f3 as periodo,
        cuentaTramosAfectados/cuentaTramosTotal as prob,
        mediaAtrasos,
        desvAtrasos
    FROM
    (
            SELECT 
                f1,
                f2,
                f3,
                count(*) as cuentaTramosTotal
            FROM
            (    
                SELECT 
                    distinct id_tramo,
                    origen as f1,
                    MONTH(fecha) as f2,
                    'MANANA' as f3
                FROM test_vuelos_atrasos.vuelos_atrasos
                WHERE 
                    YEAR(fecha) between 2005 and 2008
                    and hour(STD_UTC) in (9,10,11,12,13,14,15,16)
                    
                UNION
                
                SELECT 
                    distinct id_tramo,
                    origen as f1,
                    MONTH(fecha) as f2,
                    'TARDE' as f3
                FROM test_vuelos_atrasos.vuelos_atrasos
                WHERE 
                    YEAR(fecha) between 2005 and 2008
                    and hour(STD_UTC) in (17,18,19,20,21, 22, 23, 0)
                    
                UNION
                
                SELECT 
                    distinct id_tramo,
                    origen as f1,
                    MONTH(fecha) as f2,
                    'NOCHE' as f3
                FROM test_vuelos_atrasos.vuelos_atrasos
                WHERE 
                    YEAR(fecha) between 2005 and 2008
                    and hour(STD_UTC) in (1,2,3,4,5,6,7,8)
                    
            ) as tt
            GROUP BY f1,f2,f3
    ) as t1,

    (
        SELECT 
            f1, 
            f2,
            f3,
            count(f1) as cuentaTramosAfectados,
            avg(totalAtraso) as mediaAtrasos,
            std(totalAtraso) as desvAtrasos
        FROM
        (    
            SELECT 
                distinct id_tramo, 
                origen as f1,
                MONTH(fecha) as f2,
                'MANANA' as f3,
                sum(min_atraso) as totalAtraso
            FROM test_vuelos_atrasos.vuelos_atrasos,test_vuelos_atrasos.causas_atraso
            WHERE 
                clas1 = 'WXS'
                and causas_atraso.id = vuelos_atrasos.cod_atraso
                and YEAR(fecha) between 2005 and 2008
                and hour(STD_UTC) in (9,10,11,12,13,14,15,16)
            GROUP BY id_tramo
            
            UNION
            
                SELECT 
                distinct id_tramo, 
                origen as f1,
                MONTH(fecha) as f2,
                'TARDE' as f3,
                sum(min_atraso) as totalAtraso
            FROM test_vuelos_atrasos.vuelos_atrasos,test_vuelos_atrasos.causas_atraso
            WHERE 
                clas1 = 'WXS'
                and causas_atraso.id = vuelos_atrasos.cod_atraso
                and YEAR(fecha) between 2005 and 2008
                and hour(STD_UTC) in (17,18,19,20,21, 22, 23, 0)
            GROUP BY id_tramo
            
            UNION
            
            SELECT 
                distinct id_tramo, 
                origen as f1,
                MONTH(fecha) as f2,
                'NOCHE' as f3,
                sum(min_atraso) as totalAtraso
            FROM test_vuelos_atrasos.vuelos_atrasos,test_vuelos_atrasos.causas_atraso
            WHERE 
                clas1 = 'WXS'
                and causas_atraso.id = vuelos_atrasos.cod_atraso
                and YEAR(fecha) between 2005 and 2008
                and hour(STD_UTC) in (1,2,3,4,5,6,7,8)
            GROUP BY id_tramo
            
        ) as tt    
        GROUP BY f1,f2,f3
    ) as t2
    WHERE
        t1.f1 = t2.f1
        and t1.f2 = t2.f2
        and t1.f3 = t2.f3
        and cuentaTramosAfectados>1
) as tablaResultados
on  tablaObjetivo.aeropuerto = tablaResultados.aeropuerto and 
    tablaObjetivo.mes = tablaResultados.mes and
    tablaObjetivo.periodo = tablaResultados.periodo
order by mes,aeropuerto,periodo;   