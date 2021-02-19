const fs = require('fs')

/*
{ ID_RP: '67482   LP70B005',
     QP: 'QP067011',
     COM_NOM: null,
     TYPE_VOIE: 'ALL',
     NUMERO: '3',
     CANTON: '19',
     COM_CAPACI: null,
     ACTUALITE: '0',
     IRIS: '1701',
     CATEGORIE: 'HABIT',
     COM_DATE_C: null,
     TYPE_LOCAL: 'MAN',
     GRP_ROTATI: '2',
     ANNEE_CONS: '1974',
     DEPCOM: '67482',
     NUMERO_PAR: 'LP1460',
     LIBELLE: 'ALEXANDRE LE GRAND',
     ENSEIGNE: null,
     NOMBRE_NIV: '6',
     LISTE_INSE: 'N',
     NOMBRE_LOG: '14',
     PRINCIPAL: 'O',
     COM_SOUS_C: null,
     SOUS_TYPE: null,
     COMMENTAIR: null,
     RIVOLI: '139',
     REPETITION: null,
     DERNIER_TI: null,
     TYPE: '23',
     NUMERO_PER: null,
     ECHANTILLO: 'N',
     COM_STATUT: null,
     DATEMAJ_EA: '18/10/2016 00:00:00',
     NOMBRE_IMM: '1',
     COM_ANC_ID: null,
     ID_EA: '674820000013078',
     COMPLEMENT: 'ANCIEN 26 PL BYRON',
     COM_NB_LOG: null,
     LIEN_CMT: '1',
     apic_objec: 'symbolic',
     apic_obj00: '1',
     apic_obj01: null,
     apic_obj02: '0',
     apic_obj03: '20181122',
     apic_obj04: '102448',
     apic_obj05: '20181122',
     apic_obj06: '102448',
     apic_obj07: null,
     apic_obj08: null }
*/



let allData = JSON.parse(fs.readFileSync(__dirname+'\\RIL_2018.json'))
let dataProcessed = 0

let years = {}

allData.features.forEach(data =>{
    dataProcessed++;

    if(!data.properties['ANNEE_CONS'] ){
        data.properties['ANNEE_CONS']  = 0
    }
    
    if(!data.properties['NOMBRE_LOG'] ){
        data.properties['NOMBRE_LOG']  = 0
    }

    if(!years[data.properties['ANNEE_CONS']]){
        years[data.properties['ANNEE_CONS']] = {
            nbBat : 1,
            nbLog : Number(data.properties['NOMBRE_LOG']),
        };
    }else{
        years[data.properties['ANNEE_CONS']].nbBat += 1;
        years[data.properties['ANNEE_CONS']].nbLog += Number(data.properties['NOMBRE_LOG']);
    }
})

let res = 'YEAR,NB_BAT,NOMBRE_LOG,NB_BAT_TOTAL,NB_LOG_TOTAL\n'
let totalNbBat = 0
let totalNbLog = 0

Object.keys(years).forEach(key=>{
    totalNbBat += years[key].nbBat;
    totalNbLog += years[key].nbLog;
    res += 
    key + ',' + 
    years[key].nbBat + ',' + 
    years[key].nbLog + ',' + 
    totalNbBat + ',' + 
    totalNbLog + ',' + 
    '\n'
})

console.log('Done ! Processed : ' + dataProcessed)

fs.writeFileSync('result.csv',res)