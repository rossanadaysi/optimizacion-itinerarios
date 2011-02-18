SELECT tramo,flota_op,avg(HBT) as media,std(HBT) as desvest from
(
    SELECT distinct id_tramo,tramo,flota_op,ATA,ATD,
    hour(ATA)*60 + minute(ATA) as minutesATA, 
    hour(ATD)*60 + minute(ATD) as minutesATD,
    mod(1440 + hour(ATA)*60 + minute(ATA) - (hour(ATD)*60 + minute(ATD)),1440) as HBT
    FROM `test_vuelos_atrasos`.`vuelos_atrasos`
) as tt
group by tramo,flota_op
order by tramo,flota_op