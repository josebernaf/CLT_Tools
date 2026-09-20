using System;
using System.Collections.Generic;

namespace CLT_Tools
{
    /// <summary>
    /// Base de datos estática con los valores del Anexo 1 de la NCSE-02.
    /// </summary>
    public static class NCSE_Database
    {
        // Diccionario estático para búsqueda rápida (O(1))
        private static readonly Dictionary<string, (double ab, double K)> _cities;

        // Constructor estático: Se ejecuta una sola vez al iniciar el programa
        static NCSE_Database()
        {
            _cities = new Dictionary<string, (double, double)>();

            // =================================================================
            // ANDALUCÍA
            // =================================================================

            // --- ALMERÍA ---
            Add("Abla", 0.15); Add("Abrucena", 0.15); Add("Adra", 0.20); Add("Albanchez", 0.10);
            Add("Alboloduy", 0.15); Add("Albox", 0.10); Add("Alcolea", 0.17); Add("Alcontar", 0.09);
            Add("Alcudia de Monteagud", 0.11); Add("Alhabia", 0.15); Add("Alhama de Almeria", 0.16); Add("Alicun", 0.16);
            Add("Almeria", 0.22); Add("Almocita", 0.17); Add("Alsodux", 0.16); Add("Antas", 0.11);
            Add("Arboleas", 0.10); Add("Armuna de Almanzora", 0.09); Add("Bacares", 0.11); Add("Bayarcal", 0.17);
            Add("Bayarque", 0.09); Add("Bedar", 0.12); Add("Beires", 0.17); Add("Benahadux", 0.17);
            Add("Benitagla", 0.11); Add("Benizalon", 0.12); Add("Bentarique", 0.16); Add("Berja", 0.20);
            Add("Canjayar", 0.17); Add("Cantoria", 0.10); Add("Carboneras", 0.13); Add("Castro de Filabres", 0.12);
            Add("Chercos", 0.11); Add("Chirivel", 0.07); Add("Cobdar", 0.11); Add("Cuevas del Almanzora", 0.11);
            Add("Dalias", 0.20); Add("Ejido", 0.18); Add("Enix", 0.17); Add("Felix", 0.17);
            Add("Fines", 0.10); Add("Finana", 0.15); Add("Fondon", 0.17); Add("Gador", 0.16);
            Add("Gallardos", 0.12); Add("Garrucha", 0.11); Add("Gergal", 0.13); Add("Huecija", 0.16);
            Add("Huercal de Almeria", 0.17); Add("Huercal-Overa", 0.09); Add("Illar", 0.16); Add("Instincion", 0.16);
            Add("Laroya", 0.10); Add("Laujar de Andarax", 0.16); Add("Lijar", 0.10); Add("Lubrin", 0.12);
            Add("Lucainena de las Torres", 0.13); Add("Lucar", 0.09); Add("Macael", 0.10); Add("Maria", 0.07);
            Add("Mojacar", 0.11); Add("Mojonera", 0.17); Add("Nacimiento", 0.15); Add("Nijar", 0.16);
            Add("Ohanes", 0.17); Add("Olula de Castro", 0.12); Add("Olula del Rio", 0.10); Add("Oria", 0.09);
            Add("Padules", 0.17); Add("Partaloa", 0.09); Add("Paterna del Rio", 0.17); Add("Peches", 0.16);
            Add("Pulpi", 0.10); Add("Purchena", 0.10); Add("Ragol", 0.16); Add("Rioja", 0.16);
            Add("Roquetas de Mar", 0.16); Add("Santa Cruz de Marchena", 0.16); Add("Santa Fe de Mondujar", 0.16); Add("Senes", 0.12);
            Add("Seron", 0.09); Add("Sierro", 0.09); Add("Somontin", 0.09); Add("Sorbas", 0.13);
            Add("Sufli", 0.09); Add("Tabernas", 0.14); Add("Taberno", 0.09); Add("Tahal", 0.11);
            Add("Terque", 0.16); Add("Tijola", 0.09); Add("Tres Villas", 0.15); Add("Turre", 0.12);
            Add("Turrillas", 0.13); Add("Uleila del Campo", 0.12); Add("Urracal", 0.09); Add("Velefique", 0.12);
            Add("Velez-Blanco", 0.07); Add("Velez-Rubio", 0.07); Add("Vera", 0.10); Add("Viator", 0.17);
            Add("Vicar", 0.17); Add("Zurgena", 0.10);

            // --- CÁDIZ (Zona K Variable) ---
            Add("Alcala de los Gazules", 0.06, 1.1); Add("Alcala del Valle", 0.06); Add("Algar", 0.06, 1.1); Add("Algeciras", 0.07, 1.1);
            Add("Algodonales", 0.06); Add("Arcos de la Frontera", 0.07, 1.1); Add("Barbate", 0.06, 1.1); Add("Barrios", 0.07, 1.1);
            Add("Benalup-Casas Viejas", 0.06, 1.1); Add("Benaocaz", 0.06); Add("Bornos", 0.07, 1.1); Add("Bosque", 0.06);
            Add("Cadiz", 0.07, 1.1); Add("Castellar de la Frontera", 0.07, 1.1); Add("Chiclana de la Frontera", 0.07, 1.1); Add("Chipiona", 0.07, 1.1);
            Add("Conil de la Frontera", 0.07, 1.1); Add("Espera", 0.07, 1.1); Add("Gastor", 0.06); Add("Grazalema", 0.06);
            Add("Jerez de la Frontera", 0.07, 1.1); Add("Jimena de la Frontera", 0.07); Add("Linea de la Concepcion", 0.07, 1.1); Add("Medina-Sidonia", 0.07, 1.1);
            Add("Olvera", 0.06); Add("Paterna de Rivera", 0.07, 1.1); Add("Prado del Rey", 0.06, 1.1); Add("Puerto de Santa Maria", 0.07, 1.1);
            Add("Puerto Real", 0.07, 1.1); Add("Puerto Serrano", 0.07, 1.1); Add("Rota", 0.07, 1.1); Add("San Fernando", 0.07, 1.1);
            Add("San Jose del Valle", 0.06, 1.1); Add("San Roque", 0.07, 1.1); Add("Sanlucar de Barrameda", 0.07, 1.1); Add("Setenil", 0.06);
            Add("Tarifa", 0.07, 1.1); Add("Torre Alhaquime", 0.06); Add("Trebujena", 0.07, 1.1); Add("Ubrique", 0.06);
            Add("Vejer de la Frontera", 0.07, 1.1); Add("Villaluenga del Rosario", 0.06); Add("Villamartin", 0.07, 1.1); Add("Zahara", 0.06);

            // --- CÓRDOBA ---
            Add("Adamuz", 0.05, 1.1); Add("Aguilar de la Frontera", 0.06); Add("Almedinilla", 0.10); Add("Almodovar del Rio", 0.05, 1.1);
            Add("Baena", 0.07); Add("Benameji", 0.08); Add("Bujalance", 0.06); Add("Cabra", 0.07);
            Add("Canete de las Torres", 0.06); Add("Carcabuey", 0.09); Add("Carlota", 0.06, 1.1); Add("Carpio", 0.05);
            Add("Castro del Rio", 0.06); Add("Cordoba", 0.05, 1.1); Add("Dona Mencia", 0.07); Add("Encinas Reales", 0.08);
            Add("Espejo", 0.06); Add("Fernan-Nunez", 0.06); Add("Fuente Palmera", 0.06, 1.1); Add("Fuente Tojar", 0.09);
            Add("Guadalcazar", 0.06, 1.1); Add("Hornachuelos", 0.05, 1.1); Add("Iznajar", 0.10); Add("Lucena", 0.08);
            Add("Luque", 0.07); Add("Montalban de Cordoba", 0.06); Add("Montemayor", 0.06); Add("Montilla", 0.06);
            Add("Montoro", 0.05); Add("Monturque", 0.07); Add("Moriles", 0.07); Add("Nueva Carteya", 0.06);
            Add("Obejo", 0.04, 1.1); Add("Palenciana", 0.08); Add("Palma del Rio", 0.06, 1.1); Add("Pedro Abad", 0.05);
            Add("Posadas", 0.06, 1.1); Add("Priego de Cordoba", 0.09); Add("Puente Genil", 0.06); Add("Rambla", 0.06);
            Add("Rute", 0.09); Add("San Sebastian de los Ballesteros", 0.06); Add("Santaella", 0.06); Add("Valenzuela", 0.06);
            Add("Victoria", 0.06); Add("Villa del Rio", 0.05); Add("Villafranca de Cordoba", 0.05); Add("Villaharta", 0.04, 1.1);
            Add("Villaviciosa de Cordoba", 0.04, 1.1); Add("Zuheros", 0.07);

            // --- GRANADA ---
            Add("Agron", 0.22); Add("Alamedilla", 0.09); Add("Albolote", 0.23); Add("Albondon", 0.14);
            Add("Albunan", 0.13); Add("Albunol", 0.14); Add("Albunuelas", 0.22); Add("Aldeire", 0.13);
            Add("Alfacar", 0.22); Add("Algarinejo", 0.12); Add("Alhama de Granada", 0.23); Add("Alhendin", 0.24);
            Add("Alicun de Ortega", 0.08); Add("Almegijar", 0.15); Add("Almunecar", 0.16); Add("Alpujarra de la Sierra", 0.14);
            Add("Alquife", 0.13); Add("Arenas del Rey", 0.24); Add("Armilla", 0.24); Add("Atarfe", 0.23);
            Add("Baza", 0.12); Add("Beas de Granada", 0.20); Add("Beas de Guadix", 0.12); Add("Benalua", 0.11);
            Add("Benalua de las Villas", 0.16); Add("Benamaurel", 0.12); Add("Berchules", 0.15); Add("Bubion", 0.17);
            Add("Busquistar", 0.15); Add("Cacin", 0.24); Add("Cadiar", 0.14); Add("Cajar", 0.23);
            Add("Calahorra", 0.13); Add("Calicasas", 0.21); Add("Campotejar", 0.13); Add("Caniles", 0.13);
            Add("Canar", 0.18); Add("Capileira", 0.17); Add("Carataunas", 0.17); Add("Castaras", 0.15);
            Add("Castillejar", 0.11); Add("Castril", 0.09); Add("Cenes de la Vega", 0.22); Add("Chauchina", 0.23);
            Add("Chimeneas", 0.24); Add("Churriana de la Vega", 0.24); Add("Cijuela", 0.23); Add("Cogollos de Guadix", 0.13);
            Add("Cogollos de la Vega", 0.21); Add("Colomera", 0.18); Add("Cortes de Baza", 0.11); Add("Cortes y Graena", 0.12);
            Add("Cuevas del Campo", 0.10); Add("Cullar", 0.13); Add("Cullar Vega", 0.24); Add("Darro", 0.12);
            Add("Dehesas de Guadix", 0.09); Add("Deifontes", 0.19); Add("Diezma", 0.14); Add("Dilar", 0.24);
            Add("Dolar", 0.13); Add("Dudar", 0.21); Add("Durcal", 0.24); Add("Escuzar", 0.25);
            Add("Ferreira", 0.13); Add("Fonelas", 0.10); Add("Freila", 0.11); Add("Fuente Vaqueros", 0.23);
            Add("Gabias", 0.24); Add("Galera", 0.12); Add("Gobernador", 0.10); Add("Gojar", 0.24);
            Add("Gor", 0.12); Add("Gorafe", 0.10); Add("Granada", 0.23); Add("Guadahortuna", 0.09);
            Add("Guadix", 0.12); Add("Guajares", 0.18); Add("Gualchos", 0.13); Add("Guejar Sierra", 0.20); Add("Guevejar", 0.21);
            Add("Huelago", 0.11); Add("Hueneja", 0.14); Add("Huescar", 0.11); Add("Huetor de Santillan", 0.21);
            Add("Huetor Tajar", 0.18); Add("Huetor Vega", 0.23); Add("Illora", 0.19); Add("Itrabo", 0.18);
            Add("Iznalloz", 0.16); Add("Jayena", 0.24); Add("Jerez del Marquesado", 0.13); Add("Jete", 0.18);
            Add("Jun", 0.22); Add("Juviles", 0.15); Add("Lachar", 0.23); Add("Lanjaron", 0.18);
            Add("Lanteira", 0.13); Add("Lecrin", 0.21); Add("Lenteji", 0.20); Add("Lobras", 0.15);
            Add("Loja", 0.16); Add("Lugros", 0.14); Add("Lujar", 0.14); Add("Malaha", 0.24);
            Add("Maracena", 0.23); Add("Marchal", 0.12); Add("Moclin", 0.19); Add("Molvizar", 0.17);
            Add("Monachil", 0.23); Add("Montefrio", 0.15); Add("Montejicar", 0.10); Add("Montillana", 0.12);
            Add("Moraleda de Zafayona", 0.21); Add("Morelabor", 0.11); Add("Motril", 0.14); Add("Murtas", 0.14);
            Add("Nevada", 0.14); Add("Niguelas", 0.21); Add("Nivar", 0.21); Add("Ogijares", 0.24);
            Add("Orce", 0.13); Add("Orgiva", 0.17); Add("Otivar", 0.19); Add("Otura", 0.24);
            Add("Padul", 0.24); Add("Pampaneira", 0.17); Add("Pedro Martinez", 0.09); Add("Peligros", 0.23);
            Add("Peza", 0.19); Add("Pinar", 0.19); Add("Pinos Genil", 0.22); Add("Pinos Puente", 0.22);
            Add("Pitarres", 0.12); Add("Policar", 0.13); Add("Polopos", 0.14); Add("Portugos", 0.16);
            Add("Puebla de Don Fadrique", 0.08); Add("Pulianas", 0.22); Add("Purullena", 0.12); Add("Quentar", 0.20);
            Add("Rubite", 0.14); Add("Salar", 0.19); Add("Salobrena", 0.15); Add("Santa Cruz del Comercio", 0.23);
            Add("Santa Fe", 0.24); Add("Soportujar", 0.17); Add("Sorvilan", 0.14); Add("Taha", 0.16);
            Add("Torre-Cardela", 0.10); Add("Torvizcon", 0.15); Add("Trevelez", 0.16); Add("Turon", 0.14);
            Add("Ugijar", 0.14); Add("Valle de Lecrin", 0.21); Add("Valle del Zalabi", 0.12); Add("Valor", 0.14);
            Add("Vegas del Genil", 0.24); Add("Velez de Benaudalla", 0.17); Add("Ventas de Huelma", 0.24);
            Add("Villamena", 0.22); Add("Villanueva de las Torres", 0.09); Add("Villanueva Mesia", 0.19);
            Add("Viznar", 0.21); Add("Zafarraya", 0.20); Add("Zagra", 0.11); Add("Zubia", 0.24);
            Add("Zujar", 0.10);

            // --- HUELVA (Atención K) ---
            Add("Alajar", 0.06, 1.2); Add("Aljaraque", 0.09, 1.3); Add("Almendro", 0.08, 1.3); Add("Almonaster la Real", 0.06, 1.2);
            Add("Almonte", 0.07, 1.2); Add("Alosno", 0.08, 1.3); Add("Aracena", 0.06, 1.3); Add("Aroche", 0.07, 1.3);
            Add("Arroyomolinos de Leon", 0.05, 1.3); Add("Ayamonte", 0.10, 1.3); Add("Beas", 0.09, 1.2); Add("Berrocal", 0.07, 1.2);
            Add("Bollullos Par del Condado", 0.08, 1.2); Add("Bonares", 0.09, 1.2); Add("Cabezas Rubias", 0.09, 1.3); Add("Cala", 0.05, 1.3);
            Add("Calanas", 0.08, 1.3); Add("Campillo", 0.07, 1.3); Add("Campofrio", 0.06, 1.3); Add("Canaveral de Leon", 0.05, 1.3);
            Add("Cartaya", 0.11, 1.3); Add("Castano del Robledo", 0.06, 1.3); Add("Cerro de Andevalo", 0.08, 1.3); Add("Chucena", 0.08, 1.2);
            Add("Corteconcepcion", 0.06, 1.3); Add("Cortegana", 0.07, 1.3); Add("Cortelazor", 0.06, 1.3); Add("Cumbres de Enmedio", 0.06, 1.3);
            Add("Cumbres de San Bartolome", 0.06, 1.3); Add("Cumbres Mayores", 0.06, 1.3); Add("Encinasola", 0.06, 1.3); Add("Escacena del Campo", 0.08, 1.2);
            Add("Fuenteheridos", 0.06, 1.3); Add("Galaroza", 0.06, 1.3); Add("Gibraleon", 0.10, 1.3); Add("Granada de Rio-Tinto", 0.06, 1.3);
            Add("Granado", 0.12, 1.3); Add("Higuera de la Sierra", 0.06, 1.3); Add("Hinojales", 0.06, 1.3); Add("Hinojos", 0.08, 1.2);
            Add("Huelva", 0.10, 1.3); Add("Isla Cristina", 0.13, 1.3); Add("Jabugo", 0.06, 1.3); Add("Lepe", 0.12, 1.3);
            Add("Linares de la Sierra", 0.06, 1.3); Add("Lucena del Puerto", 0.09, 1.2); Add("Manzanilla", 0.08, 1.2); Add("Marines", 0.06, 1.3);
            Add("Minas de Riotinto", 0.07, 1.3); Add("Moguer", 0.10, 1.2); Add("Nava", 0.06, 1.3); Add("Nerva", 0.07, 1.3);
            Add("Niebla", 0.09, 1.2); Add("Palma del Condado", 0.08, 1.2); Add("Palos de la Frontera", 0.10, 1.3); Add("Paterna del Campo", 0.08, 1.2);
            Add("Paymogo", 0.11, 1.3); Add("Puebla de Guzman", 0.10, 1.3); Add("Puerto Moral", 0.06, 1.3); Add("Punta Umbria", 0.10, 1.3);
            Add("Rociana del Condado", 0.09, 1.2); Add("Rosal de la Frontera", 0.09, 1.3); Add("San Bartolome de la Torre", 0.10, 1.3); Add("San Juan del Puerto", 0.09, 1.2);
            Add("San Silvestre de Guzman", 0.12, 1.3); Add("Sanlucar de Guadiana", 0.13, 1.3); Add("Santa Ana la Real", 0.06, 1.3); Add("Santa Barbara de Casa", 0.09, 1.3);
            Add("Santa Olalla del Cala", 0.05, 1.3); Add("Trigueros", 0.09, 1.2); Add("Valdelarco", 0.06, 1.3); Add("Valverde del Camino", 0.08, 1.2);
            Add("Villablanca", 0.13, 1.3); Add("Villalba del Alcor", 0.08, 1.2); Add("Villanueva de las Cruces", 0.09, 1.3); Add("Villanueva de los Castillejos", 0.11, 1.3);
            Add("Villarrasa", 0.08, 1.2); Add("Zalamea la Real", 0.07, 1.3); Add("Zufre", 0.06, 1.3);

            // --- JAÉN ---
            Add("Albanchez de Magina", 0.07); Add("Alcala la Real", 0.12); Add("Alcaudete", 0.08); Add("Andujar", 0.05);
            Add("Arjona", 0.06); Add("Arjonilla", 0.05); Add("Arquillos", 0.04); Add("Baeza", 0.06);
            Add("Bailen", 0.05); Add("Banos de la Encina", 0.04); Add("Beas de Segura", 0.04); Add("Bedmar y Garciez", 0.06);
            Add("Begijar", 0.06); Add("Belmez de la Moraleda", 0.07); Add("Cabra del Santo Cristo", 0.07); Add("Cambil", 0.08);
            Add("Campillo de Arenas", 0.10); Add("Canena", 0.05); Add("Carboneros", 0.04); Add("Carcheles", 0.09);
            Add("Castellar", 0.04); Add("Castillo de Locubin", 0.10); Add("Cazalilla", 0.05); Add("Cazorla", 0.06);
            Add("Chilluevar", 0.06); Add("Escanuela", 0.06); Add("Espeluy", 0.05); Add("Frailes", 0.12);
            Add("Fuensanta de Martos", 0.08); Add("Fuerte del Rey", 0.06); Add("Guardia de Jaen", 0.07); Add("Guarroman", 0.04);
            Add("Higuera de Calatrava", 0.06); Add("Hinojares", 0.08); Add("Hornos", 0.04); Add("Huelma", 0.08);
            Add("Huesa", 0.07); Add("Ibros", 0.05); Add("Iruela", 0.06); Add("Iznatoraf", 0.05);
            Add("Jabalquinto", 0.05); Add("Jaen", 0.07); Add("Jamilena", 0.07); Add("Jimena", 0.06);
            Add("Jodar", 0.06); Add("Lahiguera", 0.05); Add("Larva", 0.07); Add("Linares", 0.05);
            Add("Lopera", 0.05); Add("Lupion", 0.06); Add("Mancha Real", 0.07); Add("Marmolejo", 0.05);
            Add("Martos", 0.07); Add("Mengibar", 0.06); Add("Navas de San Juan", 0.04); Add("Noalejo", 0.11);
            Add("Peal de Becerro", 0.06); Add("Pegalajar", 0.07); Add("Porcuna", 0.06); Add("Pozo Alcon", 0.08);
            Add("Quesada", 0.07); Add("Rus", 0.05); Add("Sabiote", 0.05); Add("Santiago de Calatrava", 0.06);
            Add("Santiago-Pontones", 0.05); Add("Santisteban del Puerto", 0.04); Add("Santo Tome", 0.06); Add("Sorihuela del Guadalimar", 0.04);
            Add("Torre del Campo", 0.07); Add("Torreblascopedro", 0.06); Add("Torredonjimeno", 0.07); Add("Torreperojil", 0.05);
            Add("Torres", 0.07); Add("Ubeda", 0.06); Add("Valdepenas de Jaen", 0.09); Add("Vilches", 0.04);
            Add("Villacarrillo", 0.05); Add("Villanueva de la Reina", 0.05); Add("Villanueva del Arzobispo", 0.04); Add("Villardompardo", 0.06);
            Add("Villares", 0.08); Add("Villatorres", 0.06);

            // --- MÁLAGA ---
            Add("Alameda", 0.08); Add("Alcaucin", 0.21); Add("Alfarnate", 0.16); Add("Alfarnatejo", 0.16);
            Add("Algarrobo", 0.18); Add("Algatocin", 0.07); Add("Alhaurin de la Torre", 0.08); Add("Alhaurin el Grande", 0.08);
            Add("Almachar", 0.16); Add("Almargen", 0.08); Add("Almogia", 0.09); Add("Alora", 0.08);
            Add("Alozaina", 0.08); Add("Alpandeire", 0.07); Add("Antequera", 0.09); Add("Archez", 0.21);
            Add("Archidona", 0.11); Add("Ardales", 0.08); Add("Arenas", 0.20); Add("Arriate", 0.08);
            Add("Atajate", 0.07); Add("Benadalid", 0.07); Add("Benahavis", 0.07); Add("Benalauria", 0.07);
            Add("Benalmadena", 0.08); Add("Benamargosa", 0.17); Add("Benamocarra", 0.17); Add("Benaojan", 0.07);
            Add("Benarraba", 0.07); Add("Borge", 0.16); Add("Burgo", 0.08); Add("Campillos", 0.08);
            Add("Canillas de Aceituno", 0.21); Add("Canillas de Albaida", 0.21); Add("Canete la Real", 0.08); Add("Carratraca", 0.08);
            Add("Cartajima", 0.07); Add("Cartama", 0.08); Add("Casabermeja", 0.11); Add("Casarabonela", 0.08);
            Add("Casares", 0.07, 1.1); Add("Coin", 0.07); Add("Colmenar", 0.14); Add("Comares", 0.16);
            Add("Competa", 0.21); Add("Cortes de la Frontera", 0.07); Add("Cuevas Bajas", 0.09); Add("Cuevas de San Marcos", 0.09);
            Add("Cuevas del Becerro", 0.08); Add("Cutar", 0.17); Add("Estepona", 0.07, 1.1); Add("Farajan", 0.07);
            Add("Frigiliana", 0.19); Add("Fuengirola", 0.07); Add("Fuente de Piedra", 0.08); Add("Gaucin", 0.07, 1.1);
            Add("Genalguacil", 0.07); Add("Guaro", 0.07); Add("Humilladero", 0.08); Add("Igualeja", 0.08);
            Add("Istan", 0.07); Add("Iznate", 0.16); Add("Jimera de Libar", 0.07); Add("Jubrique", 0.07);
            Add("Juzcar", 0.07); Add("Macharaviaya", 0.15); Add("Malaga", 0.11); Add("Manilva", 0.08, 1.2);
            Add("Marbella", 0.07); Add("Mijas", 0.07); Add("Moclinejo", 0.15); Add("Mollina", 0.08);
            Add("Monda", 0.08); Add("Montejaque", 0.07); Add("Nerja", 0.17); Add("Ojen", 0.07);
            Add("Parauta", 0.08); Add("Periana", 0.19); Add("Pizarra", 0.08); Add("Pujerra", 0.07);
            Add("Rincon de la Victoria", 0.12); Add("Riogordo", 0.16); Add("Ronda", 0.08); Add("Salares", 0.21);
            Add("Sayalonga", 0.19); Add("Sedella", 0.21); Add("Sierra de Yeguas", 0.08); Add("Teba", 0.08);
            Add("Tolox", 0.08); Add("Torremolinos", 0.08); Add("Torrox", 0.18); Add("Totalan", 0.13);
            Add("Valle de Abdalajis", 0.08); Add("Velez-Malaga", 0.18); Add("Villanueva de Algaidas", 0.09); Add("Villanueva de Tapia", 0.11);
            Add("Villanueva del Rosario", 0.13); Add("Villanueva del Trabuco", 0.13); Add("Vinuela", 0.19); Add("Yunquera", 0.08);

            // --- SEVILLA ---
            Add("Aguadulce", 0.07); Add("Alanis", 0.04, 1.2); Add("Albaida del Aljarafe", 0.07, 1.1); Add("Alcala de Guadaira", 0.06, 1.1);
            Add("Alcala del Rio", 0.09); Add("Alcolea del Rio", 0.06, 1.1); Add("Algaba", 0.07, 1.2); Add("Algamitas", 0.08);
            Add("Almaden de la Plata", 0.05, 1.2); Add("Almensilla", 0.07, 1.1); Add("Arahal", 0.06, 1.1); Add("Aznalcazar", 0.08, 1.2);
            Add("Aznalcollar", 0.07, 1.2); Add("Badolatosa", 0.07); Add("Benacazon", 0.08, 1.1); Add("Bollullos de la Mitacion", 0.07, 1.1);
            Add("Bormujos", 0.07, 1.1); Add("Brenes", 0.06, 1.1); Add("Burguillos", 0.06, 1.1); Add("Cabezas de San Juan", 0.07, 1.1);
            Add("Camas", 0.07, 1.2); Add("Campana", 0.06, 1.1); Add("Cantillana", 0.06, 1.1); Add("Canada Rosal", 0.06, 1.1);
            Add("Carmona", 0.06, 1.1); Add("Carrion de los Cespedes", 0.06, 1.1); Add("Casariche", 0.07); Add("Castilblanco de los Arroyos", 0.06, 1.2);
            Add("Castilleja de Guzman", 0.07, 1.2); Add("Castilleja de la Cuesta", 0.07, 1.1); Add("Castilleja del Campo", 0.07); Add("Castillo de las Guardas", 0.07, 1.2);
            Add("Cazalla de la Sierra", 0.05, 1.2); Add("Constantina", 0.05, 1.1); Add("Coria del Rio", 0.07, 1.1); Add("Coripe", 0.08);
            Add("Coronil", 0.07, 1.1); Add("Corrales", 0.08); Add("Cuervo de Sevilla", 0.06, 1.2); Add("Dos Hermanas", 0.07, 1.1);
            Add("Ecija", 0.06, 1.1); Add("Espartinas", 0.07, 1.1); Add("Estepa", 0.07); Add("Fuentes de Andalucia", 0.06, 1.1);
            Add("Garrobo", 0.07, 1.2); Add("Gelves", 0.07, 1.1); Add("Gerena", 0.07, 1.2); Add("Gilena", 0.07);
            Add("Gines", 0.07, 1.1); Add("Guadalcanal", 0.04, 1.2); Add("Guillena", 0.07, 1.2); Add("Herrera", 0.06);
            Add("Huevar de Aljarafe", 0.08, 1.2); Add("Isla Mayor", 0.08, 1.2); Add("Lantejuela", 0.06, 1.1); Add("Lebrija", 0.06, 1.2);
            Add("Lora de Estepa", 0.07); Add("Lora del Rio", 0.06, 1.1); Add("Luisiana", 0.06, 1.1); Add("Madrono", 0.07, 1.2);
            Add("Mairena del Alcor", 0.06, 1.1); Add("Mairena del Aljarafe", 0.07, 1.1); Add("Marchena", 0.06, 1.1); Add("Marinaleda", 0.06);
            Add("Martin de la Jara", 0.08); Add("Molares", 0.06, 1.1); Add("Montellano", 0.07, 1.1); Add("Moron de la Frontera", 0.07, 1.1);
            Add("Navas de la Concepcion", 0.05, 1.1); Add("Olivares", 0.07, 1.1); Add("Osuna", 0.07); Add("Palacios y Villafranca", 0.07, 1.1);
            Add("Palomares del Rio", 0.07, 1.1); Add("Paradas", 0.06, 1.1); Add("Pedrera", 0.07); Add("Pedroso", 0.05, 1.1);
            Add("Penaflor", 0.06, 1.1); Add("Pilas", 0.08, 1.2); Add("Pruna", 0.08); Add("Puebla de Cazalla", 0.06, 1.1);
            Add("Puebla de los Infantes", 0.06, 1.1); Add("Puebla del Rio", 0.07, 1.1); Add("Real de la Jara", 0.05, 1.2); Add("Rinconada", 0.07, 1.1);
            Add("Roda de Andalucia", 0.07); Add("Ronquillo", 0.06, 1.2); Add("Rubio", 0.06); Add("Salteras", 0.07, 1.2);
            Add("San Juan de Aznalfarache", 0.07, 1.1); Add("San Nicolas del Puerto", 0.04, 1.2); Add("Sanlucar la Mayor", 0.08, 1.1); Add("Santiponce", 0.07, 1.2);
            Add("Saucejo", 0.08); Add("Sevilla", 0.07, 1.1); Add("Tocina", 0.06, 1.1); Add("Tomares", 0.07, 1.1);
            Add("Umbrete", 0.07, 1.1); Add("Utrera", 0.06, 1.1); Add("Valencina de la Concepcion", 0.07, 1.2); Add("Villamanrique de la Condesa", 0.08, 1.2);
            Add("Villanueva de San Juan", 0.08); Add("Villanueva del Ariscal", 0.07, 1.1); Add("Villanueva del Rio y Minas", 0.06, 1.1); Add("Villaverde del Rio", 0.06, 1.1);
            Add("Viso del Alcor", 0.06, 1.1);

            // =================================================================
            // ARAGÓN
            // =================================================================
            // HUESCA
            Add("Ainsa Sobrarbe", 0.05); Add("Aisa", 0.05); Add("Anso", 0.05); Add("Aragues del Puerto", 0.05);
            Add("Benasque", 0.05); Add("Bielsa", 0.10); Add("Biescas", 0.07); Add("Bisaurri", 0.04);
            Add("Boltana", 0.05); Add("Borau", 0.05); Add("Broto", 0.08); Add("Campo", 0.04);
            Add("Canal de Berdun", 0.04); Add("Canfranc", 0.07); Add("Castejon de Sos", 0.04); Add("Castiello de Jaca", 0.05);
            Add("Chia", 0.05); Add("Fago", 0.05); Add("Fanlo", 0.09); Add("Fiscal", 0.05);
            Add("Foradada del Toscar", 0.04); Add("Fueva", 0.04); Add("Gistain", 0.06); Add("Hoz de Jaca", 0.09);
            Add("Jaca", 0.04); Add("Jasa", 0.05); Add("Labuerda", 0.06); Add("Laspuna", 0.07);
            Add("Llert", 0.04); Add("Palo", 0.04); Add("Panticosa", 0.10); Add("Plan", 0.08);
            Add("Puente la Reina de Jaca", 0.04); Add("Puertolas", 0.08); Add("Pueyo de Araguas", 0.05); Add("Sabinanigo", 0.04);
            Add("Sahun", 0.05); Add("Sallent de Gallego", 0.10); Add("San Juan de Plan", 0.08); Add("Santa Cilia de Jaca", 0.04);
            Add("Santa Cruz de la Seros", 0.04); Add("Seira", 0.04); Add("Sesue", 0.05); Add("Tella Sin", 0.09);
            Add("Torla", 0.09); Add("Valle de Hecho", 0.06); Add("Villanova", 0.05); Add("Villanua", 0.06);
            Add("Yebra de Basa", 0.04); Add("Yesero", 0.07);

            // ZARAGOZA
            Add("Artieda", 0.04); Add("Bagues", 0.04); Add("Mianos", 0.04); Add("Navardun", 0.04);
            Add("Pintanos", 0.04); Add("Salvatierra de Esca", 0.05); Add("Sigues", 0.04); Add("Undues de Lerda", 0.04);
            Add("Urries", 0.04);

            // =================================================================
            // CANARIAS
            // =================================================================
            // LAS PALMAS
            Add("Agaete", 0.04); Add("Aguimes", 0.04); Add("Antigua", 0.04); Add("Arrecife", 0.04);
            Add("Artenara", 0.04); Add("Arucas", 0.04); Add("Betancuria", 0.04); Add("Firgas", 0.04);
            Add("Galdar", 0.04); Add("Haria", 0.04); Add("Ingenio", 0.04); Add("Mogan", 0.04);
            Add("Moya", 0.04); Add("Oliva", 0.04); Add("Pajara", 0.04); Add("Palmas de Gran Canaria", 0.04);
            Add("Puerto del Rosario", 0.04); Add("San Bartolome", 0.04); Add("San Bartolome de Tirajana", 0.04); Add("San Nicolas de Tolentino", 0.04);
            Add("Santa Brigida", 0.04); Add("Santa Lucia de Tirajana", 0.04); Add("Santa Maria de Guia", 0.04); Add("Teguise", 0.04);
            Add("Tejeda", 0.04); Add("Telde", 0.04); Add("Teror", 0.04); Add("Tias", 0.04);
            Add("Tinajo", 0.04); Add("Tuineje", 0.04); Add("Valleseco", 0.04); Add("Valsequillo", 0.04);
            Add("Vega de San Mateo", 0.04); Add("Yaiza", 0.04);

            // SANTA CRUZ DE TENERIFE
            Add("Adeje", 0.04); Add("Agulo", 0.04); Add("Alajero", 0.04); Add("Arafo", 0.04);
            Add("Arico", 0.04); Add("Arona", 0.04); Add("Barlovento", 0.04); Add("Brena Alta", 0.04);
            Add("Brena Baja", 0.04); Add("Buenavista del Norte", 0.04); Add("Candelaria", 0.04); Add("Fasnia", 0.04);
            Add("Frontera", 0.04); Add("Fuencaliente de la Palma", 0.04); Add("Garachico", 0.04); Add("Garafia", 0.04);
            Add("Granadilla de Abona", 0.04); Add("Guancha", 0.04); Add("Guia de Isora", 0.04); Add("Guimar", 0.04);
            Add("Hermigua", 0.04); Add("Icod de los Vinos", 0.04); Add("Llanos de Aridane", 0.04); Add("Matanza de Acentejo", 0.04);
            Add("Orotava", 0.04); Add("Paso", 0.04); Add("Puerto de la Cruz", 0.04); Add("Puntagorda", 0.04);
            Add("Puntallana", 0.04); Add("Realejos", 0.04); Add("Rosario", 0.04); Add("San Andres y Sauces", 0.04);
            Add("San Cristobal de la Laguna", 0.04); Add("San Juan de la Rambla", 0.04); Add("San Miguel de Abona", 0.04); Add("San Sebastian de la Gomera", 0.04);
            Add("Santa Cruz de la Palma", 0.04); Add("Santa Cruz de Tenerife", 0.04); Add("Santa Ursula", 0.04); Add("Santiago del Teide", 0.04);
            Add("Sauzal", 0.04); Add("Silos", 0.04); Add("Tacoronte", 0.04); Add("Tanque", 0.04);
            Add("Tazacorte", 0.04); Add("Tegueste", 0.04); Add("Tijarafe", 0.04); Add("Valle Gran Rey", 0.04);
            Add("Vallehermoso", 0.04); Add("Valverde", 0.04); Add("Victoria de Acentejo", 0.04); Add("Vilaflor", 0.04);
            Add("Villa de Mazo", 0.04);

            // =================================================================
            // CASTILLA - LA MANCHA
            // =================================================================
            // ALBACETE
            Add("Alatoz", 0.05); Add("Albacete", 0.04); Add("Albatana", 0.07); Add("Alcadozo", 0.05);
            Add("Alcaraz", 0.06); Add("Almansa", 0.07); Add("Alpera", 0.07); Add("Ayna", 0.05);
            Add("Balsa de Ves", 0.04); Add("Bonete", 0.07); Add("Carcelen", 0.05); Add("Caudete", 0.07);
            Add("Corral-Rubio", 0.06); Add("Elche de la Sierra", 0.06); Add("Ferez", 0.07); Add("Fuente-Alamo", 0.07);
            Add("Hellin", 0.07); Add("Higueruela", 0.05); Add("Hoya-Gonzalo", 0.05); Add("Letur", 0.06);
            Add("Lietor", 0.06); Add("Molinicos", 0.04); Add("Montealegre del Castillo", 0.07); Add("Nerpio", 0.05);
            Add("Ontur", 0.07); Add("Petrola", 0.06); Add("Pozohondo", 0.04); Add("Socovos", 0.07);
            Add("Tobarra", 0.07); Add("Villa de Ves", 0.04); Add("Yeste", 0.04);

            // =================================================================
            // CATALUÑA
            // =================================================================
            // BARCELONA
            Add("Abrera", 0.04); Add("Aiguafreda", 0.05); Add("Alella", 0.04); Add("Alpens", 0.08);
            Add("Ametlla del Valles", 0.04); Add("Arenys de Mar", 0.04); Add("Arenys de Munt", 0.04); Add("Argentona", 0.04);
            Add("Artes", 0.04); Add("Avia", 0.05); Add("Avinyo", 0.04); Add("Avinyonet del Penedes", 0.04);
            Add("Badalona", 0.04); Add("Badia del Valles", 0.04); Add("Baga", 0.07); Add("Balenya", 0.05);
            Add("Balsareny", 0.04); Add("Barbera del Valles", 0.04); Add("Barcelona", 0.04); Add("Begues", 0.04);
            Add("Bellprat", 0.04); Add("Berga", 0.06); Add("Bigues i Riells", 0.04); Add("Borreda", 0.08);
            Add("Bruc", 0.04); Add("Brull", 0.05); Add("Cabanyes", 0.04); Add("Cabrera d'Igualada", 0.04);
            Add("Cabrera de Mar", 0.04); Add("Cabrils", 0.04); Add("Calaf", 0.04); Add("Calders", 0.04);
            Add("Caldes de Montbui", 0.04); Add("Caldes d'Estrac", 0.04); Add("Calella", 0.04); Add("Calldetenes", 0.06);
            Add("Campins", 0.05); Add("Canet de Mar", 0.04); Add("Canovelles", 0.04); Add("Canoves i Samalus", 0.05);
            Add("Canyelles", 0.04); Add("Capellades", 0.04); Add("Capolat", 0.04); Add("Cardedeu", 0.04);
            Add("Carme", 0.04); Add("Casserres", 0.04); Add("Castell de l'Areny", 0.07); Add("Castellar de n'Hug", 0.08);
            Add("Castellar del Riu", 0.05); Add("Castellar del Valles", 0.04); Add("Castellbell i el Vilar", 0.04); Add("Castellbisbal", 0.04);
            Add("Castellcir", 0.04); Add("Castelldefels", 0.04); Add("Castellet i la Gornal", 0.04); Add("Castellfollit del Boix", 0.04);
            Add("Castellgali", 0.04); Add("Castelloli", 0.04); Add("Castelltercol", 0.04); Add("Castellvi de la Marca", 0.04);
            Add("Castellvi de Rosanes", 0.04); Add("Centelles", 0.05); Add("Cercs", 0.06); Add("Cerdanyola del Valles", 0.04);
            Add("Cervello", 0.04); Add("Collbato", 0.04); Add("Collsuspina", 0.05); Add("Corbera de Llobregat", 0.04);
            Add("Cornella de Llobregat", 0.04); Add("Cubelles", 0.04); Add("Dosrius", 0.04); Add("Esparreguera", 0.04);
            Add("Esplugues de Llobregat", 0.04); Add("Espunyola", 0.04); Add("Estany", 0.05); Add("Figar-Montmany", 0.04);
            Add("Figols", 0.06); Add("Fogars de la Selva", 0.05); Add("Fogars de Montclus", 0.05); Add("Folgueroles", 0.06);
            Add("Fonollosa", 0.04); Add("Font-rubi", 0.04); Add("Franqueses del Valles", 0.04); Add("Gaià", 0.04);
            Add("Gallifa", 0.04); Add("Garriga", 0.04); Add("Gava", 0.04); Add("Gelida", 0.04);
            Add("Gironella", 0.05); Add("Gisclareny", 0.07); Add("Granada", 0.04); Add("Granera", 0.04);
            Add("Granollers", 0.04); Add("Gualba", 0.05); Add("Guardiola de Bergueda", 0.08); Add("Gurb", 0.06);
            Add("Hospitalet de Llobregat", 0.04); Add("Hostalets de Pierola", 0.04); Add("Igualada", 0.04); Add("Jorba", 0.04);
            Add("Llacuna", 0.04); Add("Llagosta", 0.04); Add("Llica d'Amunt", 0.04); Add("Llica de Vall", 0.04);
            Add("Llinars del Valles", 0.04); Add("Lluca", 0.06); Add("Malgrat de Mar", 0.04); Add("Malla", 0.05);
            Add("Manlleu", 0.08); Add("Manresa", 0.04); Add("Marganell", 0.04); Add("Martorell", 0.04);
            Add("Martorelles", 0.04); Add("Masies de Roda", 0.08); Add("Masies de Voltrega", 0.08); Add("Masnou", 0.04);
            Add("Masquefa", 0.04); Add("Matadepera", 0.04); Add("Mataro", 0.04); Add("Mediona", 0.04);
            Add("Moia", 0.04); Add("Molins de Rei", 0.04); Add("Mollet del Valles", 0.04); Add("Monistrol de Calders", 0.04);
            Add("Monistrol de Montserrat", 0.04); Add("Montcada i Reixac", 0.04); Add("Montclar", 0.04); Add("Montesquiu", 0.09);
            Add("Montgat", 0.04); Add("Montmelo", 0.04); Add("Montornes del Valles", 0.04); Add("Montseny", 0.05);
            Add("Muntanyola", 0.05); Add("Mura", 0.04); Add("Navarcles", 0.04); Add("Nou de Bergueda", 0.06);
            Add("Odena", 0.04); Add("Olerdola", 0.04); Add("Olesa de Bonesvalls", 0.04); Add("Olesa de Montserrat", 0.04);
            Add("Olivella", 0.04); Add("Olost", 0.05); Add("Olvan", 0.05); Add("Oris", 0.08);
            Add("Orista", 0.05); Add("Orpi", 0.04); Add("Orrius", 0.04); Add("Pacs del Penedes", 0.04);
            Add("Palafolls", 0.04); Add("Palau-solita i Plegamans", 0.04); Add("Palleja", 0.04); Add("Palma de Cervello", 0.04);
            Add("Papiol", 0.04); Add("Parets del Valles", 0.04); Add("Perafita", 0.06); Add("Piera", 0.04);
            Add("Pineda de Mar", 0.04); Add("Pla del Penedes", 0.04); Add("Pobla de Claramunt", 0.04); Add("Pobla de Lillet", 0.08);
            Add("Polinya", 0.04); Add("Pont de Vilomara i Rocafort", 0.04); Add("Pontons", 0.04); Add("Prat de Llobregat", 0.04);
            Add("Prats de Llucanes", 0.05); Add("Premia de Dalt", 0.04); Add("Premia de Mar", 0.04); Add("Puigdalber", 0.04);
            Add("Puig-reig", 0.04); Add("Quar", 0.06); Add("Rellinars", 0.04); Add("Ripollet", 0.04);
            Add("Roca del Valles", 0.04); Add("Roda de Ter", 0.08); Add("Rubi", 0.04); Add("Rubio", 0.04);
            Add("Rupit i Pruit", 0.09); Add("Sabadell", 0.04); Add("Sagas", 0.05); Add("Saldes", 0.06);
            Add("Sallent", 0.04); Add("Sant Adria de Besos", 0.04); Add("Sant Agusti de Llucanes", 0.07); Add("Sant Andreu de la Barca", 0.04);
            Add("Sant Andreu de Llavaneres", 0.04); Add("Sant Antoni de Vilamajor", 0.04); Add("Sant Bartomeu del Grau", 0.06); Add("Sant Boi de Llobregat", 0.04);
            Add("Sant Boi de Llucanes", 0.07); Add("Sant Cebria de Vallalta", 0.04); Add("Sant Celoni", 0.05); Add("Sant Climent de Llobregat", 0.04);
            Add("Sant Cugat del Valles", 0.04); Add("Sant Cugat Sesgarrigues", 0.04); Add("Sant Esteve de Palautordera", 0.05); Add("Sant Esteve Sesrovires", 0.04);
            Add("Sant Feliu de Codines", 0.04); Add("Sant Feliu de Llobregat", 0.04); Add("Sant Feliu Sasserra", 0.04); Add("Sant Fost de Campsentelles", 0.04);
            Add("Sant Fruitos de Bages", 0.04); Add("Sant Hipolit de Voltrega", 0.07); Add("Sant Iscle de Vallalta", 0.04); Add("Sant Jaume de Frontanya", 0.08);
            Add("Sant Joan de Mediona", 0.04); Add("Sant Joan de Vilatorrada", 0.04); Add("Sant Joan Despi", 0.04); Add("Sant Julia de Cerdanyola", 0.07);
            Add("Sant Julia de Vilatorta", 0.06); Add("Sant Just Desvern", 0.04); Add("Sant Llorenc d'Hortons", 0.04); Add("Sant Llorenc Savall", 0.04);
            Add("Sant Marti d'Albars", 0.06); Add("Sant Marti de Centelles", 0.04); Add("Sant Marti de Tous", 0.04); Add("Sant Marti Sarroca", 0.04);
            Add("Sant Pere de Ribes", 0.04); Add("Sant Pere de Riudebitlles", 0.04); Add("Sant Pere de Torello", 0.09); Add("Sant Pere de Vilamajor", 0.05);
            Add("Sant Pol de Mar", 0.04); Add("Sant Quinti de Mediona", 0.04); Add("Sant Quirze de Besora", 0.09); Add("Sant Quirze del Valles", 0.04);
            Add("Sant Quirze Safaja", 0.04); Add("Sant Sadurni d'Anoia", 0.04); Add("Sant Sadurni d'Osormort", 0.06); Add("Sant Salvador de Guardiola", 0.04);
            Add("Sant Vicenc de Castellet", 0.04); Add("Sant Vicenc de Montalt", 0.04); Add("Sant Vicenc de Torello", 0.09); Add("Sant Vicenc dels Horts", 0.04);
            Add("Santa Cecilia de Voltrega", 0.07); Add("Santa Coloma de Cervello", 0.04); Add("Santa Coloma de Gramenet", 0.04); Add("Santa Eugenia de Berga", 0.06);
            Add("Santa Eulalia de Riuprimer", 0.05); Add("Santa Eulalia de Roncana", 0.04); Add("Santa Fe del Penedes", 0.04); Add("Santa Margarida de Montbui", 0.04);
            Add("Santa Margarida i els Monjos", 0.04); Add("Santa Maria de Besora", 0.09); Add("Santa Maria de Corco", 0.09); Add("Santa Maria de Martorelles", 0.04);
            Add("Santa Maria de Merles", 0.05); Add("Santa Maria de Miralles", 0.04); Add("Santa Maria de Palautordera", 0.05); Add("Santa Maria d'Olo", 0.05);
            Add("Santa Perpetua de Mogoda", 0.04); Add("Santa Susanna", 0.04); Add("Santpedor", 0.04); Add("Sentmenat", 0.04);
            Add("Seva", 0.05); Add("Sitges", 0.04); Add("Sobremunt", 0.08); Add("Sora", 0.08);
            Add("Subirats", 0.04); Add("Tagamanent", 0.05); Add("Talamanca", 0.04); Add("Taradell", 0.05);
            Add("Tavernoles", 0.07); Add("Tavertet", 0.08); Add("Teia", 0.04); Add("Terrassa", 0.04);
            Add("Tiana", 0.04); Add("Tona", 0.05); Add("Tordera", 0.05); Add("Torello", 0.09);
            Add("Torre de Claramunt", 0.04); Add("Torrelavit", 0.04); Add("Torrelles de Foix", 0.04); Add("Torrelles de Llobregat", 0.04);
            Add("Ullastrell", 0.04); Add("Vacarisses", 0.04); Add("Vallbona d'Anoia", 0.04); Add("Vallcebre", 0.06);
            Add("Vallgorguina", 0.04); Add("Vallirana", 0.04); Add("Vallromanes", 0.04); Add("Vic", 0.06);
            Add("Vilada", 0.05); Add("Viladecans", 0.04); Add("Viladecavalls", 0.04); Add("Vilafranca del Penedes", 0.04);
            Add("Vilalba Sasserra", 0.04); Add("Vilanova de Sau", 0.07); Add("Vilanova del Cami", 0.04); Add("Vilanova del Valles", 0.04);
            Add("Vilanova i la Geltru", 0.04); Add("Vilassar de Dalt", 0.04); Add("Vilassar de Mar", 0.04); Add("Vilobi del Penedes", 0.04);

            // GIRONA
            Add("Agullana", 0.09); Add("Aiguaviva", 0.07); Add("Albanya", 0.10); Add("Albons", 0.07);
            Add("Alp", 0.07); Add("Amer", 0.09); Add("Angles", 0.08); Add("Arbucies", 0.05);
            Add("Argelaguer", 0.10); Add("Armentera", 0.08); Add("Avinyonet de Puigventos", 0.09); Add("Banyoles", 0.10);
            Add("Bascara", 0.09); Add("Begur", 0.05); Add("Bellcaire d'Emporda", 0.08); Add("Besalu", 0.10);
            Add("Bescano", 0.08); Add("Beuda", 0.10); Add("Bisbal d'Emporda", 0.07); Add("Biure", 0.09);
            Add("Blanes", 0.04); Add("Boadella d'Emporda", 0.09); Add("Bolvir", 0.05); Add("Bordils", 0.07);
            Add("Borrassa", 0.09); Add("Breda", 0.05); Add("Brunyola", 0.07); Add("Cabanelles", 0.10);
            Add("Cabanes", 0.08); Add("Cadaques", 0.05); Add("Caldes de Malavella", 0.06); Add("Calonge", 0.05);
            Add("Camos", 0.10); Add("Campdevanol", 0.09); Add("Campelles", 0.09); Add("Campllong", 0.07);
            Add("Camprodon", 0.11); Add("Canet d'Adri", 0.09); Add("Cantallops", 0.09); Add("Capmany", 0.09);
            Add("Cassa de la Selva", 0.06); Add("Castellfollit de la Roca", 0.10); Add("Castello d'Empuries", 0.08); Add("Castell-Platja d'Aro", 0.05);
            Add("Cellera de Ter", 0.08); Add("Celra", 0.07); Add("Cervia de Ter", 0.08); Add("Cistella", 0.10);
            Add("Colera", 0.06); Add("Colomers", 0.08); Add("Corca", 0.07); Add("Cornella del Terri", 0.10);
            Add("Crespia", 0.10); Add("Cruilles Monells i Sant Sadurni", 0.07); Add("Darnius", 0.09); Add("Das", 0.07);
            Add("Escala", 0.07); Add("Espinelves", 0.06); Add("Espolla", 0.08); Add("Esponella", 0.10);
            Add("Far d'Emporda", 0.08); Add("Figueres", 0.09); Add("Flaca", 0.08); Add("Foixa", 0.08);
            Add("Fontanals de Cerdanya", 0.06); Add("Fontanilles", 0.06); Add("Fontcoberta", 0.10); Add("Forallac", 0.06);
            Add("Fornells de la Selva", 0.07); Add("Fortia", 0.08); Add("Garrigas", 0.09); Add("Garrigoles", 0.08);
            Add("Garriguella", 0.07); Add("Ger", 0.06); Add("Girona", 0.08); Add("Gombren", 0.09);
            Add("Gualta", 0.07); Add("Guils de Cerdanya", 0.07); Add("Hostalric", 0.05); Add("Isovol", 0.06);
            Add("Jafre", 0.08); Add("Jonquera", 0.09); Add("Juia", 0.08); Add("Llado", 0.10);
            Add("Llagostera", 0.05); Add("Llambilles", 0.07); Add("Llanars", 0.11); Add("Llanca", 0.07);
            Add("Llers", 0.09); Add("Llivia", 0.06); Add("Lloret de Mar", 0.04); Add("Llosses", 0.08);
            Add("Macanet de Cabrenys", 0.10); Add("Macanet de la Selva", 0.05); Add("Madremanya", 0.08); Add("Maia de Montcal", 0.10);
            Add("Masarac", 0.08); Add("Massanes", 0.05); Add("Meranges", 0.07); Add("Mieres", 0.10);
            Add("Mollet de Peralada", 0.08); Add("Mollo", 0.11); Add("Montagut", 0.11); Add("Mont-ras", 0.05);
            Add("Navata", 0.09); Add("Ogassa", 0.10); Add("Olot", 0.10); Add("Ordis", 0.09);
            Add("Osor", 0.08); Add("Palafrugell", 0.05); Add("Palamos", 0.04); Add("Palau de Santa Eulalia", 0.09);
            Add("Palau-sator", 0.07); Add("Palau-saverdera", 0.07); Add("Palol de Revardit", 0.10); Add("Pals", 0.06);
            Add("Pardines", 0.10); Add("Parlava", 0.07); Add("Pau", 0.07); Add("Pedret i Marza", 0.07);
            Add("Pera", 0.07); Add("Peralada", 0.08); Add("Planes d'Hostoles", 0.10); Add("Planoles", 0.09);
            Add("Pont de Molins", 0.09); Add("Pontos", 0.09); Add("Porqueres", 0.10); Add("Port de la Selva", 0.06);
            Add("Portbou", 0.06); Add("Preses", 0.10); Add("Puigcerda", 0.06); Add("Quart", 0.08);
            Add("Queralbs", 0.10); Add("Rabos", 0.08); Add("Regencos", 0.05); Add("Ribes de Freser", 0.10);
            Add("Riells i Viabrea", 0.05); Add("Ripoll", 0.09); Add("Riudarenes", 0.06); Add("Riudaura", 0.10);
            Add("Riudellots de la Selva", 0.07); Add("Riumors", 0.08); Add("Roses", 0.08); Add("Rupia", 0.08);
            Add("Sales de Llierca", 0.11); Add("Salt", 0.08); Add("Sant Andreu Salou", 0.07); Add("Sant Aniol de Finestres", 0.10);
            Add("Sant Climent Sescebes", 0.08); Add("Sant Feliu de Buixalleu", 0.05); Add("Sant Feliu de Guixols", 0.04); Add("Sant Feliu de Pallerols", 0.10);
            Add("Sant Ferriol", 0.10); Add("Sant Gregori", 0.09); Add("Sant Hilari Sacalm", 0.06); Add("Sant Jaume de Llierca", 0.10);
            Add("Sant Joan de les Abadesses", 0.10); Add("Sant Joan de Mollet", 0.08); Add("Sant Joan les Fonts", 0.11); Add("Sant Jordi Desvalls", 0.08);
            Add("Sant Julia de Ramis", 0.08); Add("Sant Julia del Llor i Bonmati", 0.09); Add("Sant Llorenc de la Muga", 0.10); Add("Sant Marti de Llemena", 0.09);
            Add("Sant Marti Vell", 0.08); Add("Sant Miquel de Campmajor", 0.10); Add("Sant Miquel de Fluvia", 0.09); Add("Sant Mori", 0.08);
            Add("Sant Pau de Seguries", 0.11); Add("Sant Pere Pescador", 0.08); Add("Santa Coloma de Farners", 0.06); Add("Santa Cristina d'Aro", 0.05);
            Add("Santa Llogaia d'Alguema", 0.09); Add("Santa Pau", 0.10); Add("Sarria de Ter", 0.08); Add("Saus", 0.08);
            Add("Selva de Mar", 0.06); Add("Serinya", 0.10); Add("Serra de Daro", 0.07); Add("Setcases", 0.11);
            Add("Sils", 0.06); Add("Siurana", 0.08); Add("Susqueda", 0.09); Add("Tallada d'Emporda", 0.08);
            Add("Terrades", 0.10); Add("Torrent", 0.06); Add("Torroella de Fluvia", 0.08); Add("Torroella de Montgri", 0.07);
            Add("Tortella", 0.11); Add("Toses", 0.09); Add("Tossa de Mar", 0.04); Add("Ullastret", 0.07);
            Add("Ultramort", 0.07); Add("Urus", 0.07); Add("Vajol", 0.09); Add("Vall de Bianya", 0.11);
            Add("Vall d'en Bas", 0.10); Add("Vallfogona de Ripolles", 0.10); Add("Vall-llobrega", 0.05); Add("Ventallo", 0.08);
            Add("Verges", 0.08); Add("Vidra", 0.10); Add("Vidreres", 0.05); Add("Vilabertran", 0.09);
            Add("Vilablareix", 0.07); Add("Viladamat", 0.08); Add("Viladasens", 0.08); Add("Vilademuls", 0.09);
            Add("Viladrau", 0.06); Add("Vilafant", 0.09); Add("Vilajuiga", 0.07); Add("Vilallonga de Ter", 0.11);
            Add("Vilamacolum", 0.08); Add("Vilamalla", 0.08); Add("Vilamaniscle", 0.07); Add("Vilanant", 0.09);
            Add("Vila-sacra", 0.08); Add("Vilaur", 0.08); Add("Vilobi d'Onyar", 0.06); Add("Vilopriu", 0.08);

            // LLEIDA
            Add("Alas i Cerc", 0.06); Add("Alins", 0.06); Add("Alt Aneu", 0.05); Add("Arres", 0.04);
            Add("Arseguel", 0.06); Add("Bausen", 0.05); Add("Bellaguarda", 0.04); Add("Bellver de Cerdanya", 0.07);
            Add("Bordes", 0.04); Add("Bossost", 0.04); Add("Canejan", 0.04); Add("Cava", 0.06);
            Add("Coma i la Pedra", 0.05); Add("Espot", 0.04); Add("Estamariu", 0.06); Add("Esterri d'Aneu", 0.05);
            Add("Esterri de Cardos", 0.06); Add("Farrera", 0.05); Add("Gosol", 0.06); Add("Guingueta d'Aneu", 0.05);
            Add("Guixers", 0.04); Add("Josa i Tuixen", 0.05); Add("Les", 0.04); Add("Lladorre", 0.06);
            Add("Llavorsi", 0.05); Add("Lles de Cerdanya", 0.07); Add("Montella i Martinet", 0.07); Add("Montferrer i Castellbo", 0.06);
            Add("Naut Aran", 0.04); Add("Pobla de Cervoles", 0.04); Add("Pont de Bar", 0.06); Add("Prats i Sansor", 0.07);
            Add("Prullans", 0.07); Add("Rialp", 0.04); Add("Ribera d'Urgellet", 0.05); Add("Sant Llorenc de Morunys", 0.04);
            Add("Seu d'Urgell", 0.06); Add("Soriguera", 0.04); Add("Sort", 0.04); Add("Tarres", 0.04);
            Add("Tirvia", 0.05); Add("Vall de Cardos", 0.05); Add("Valls d'Aguilar", 0.04); Add("Valls de Valira", 0.06);
            Add("Vansa i Fornols", 0.05); Add("Vielha e Mijaran", 0.04); Add("Vilamos", 0.04); Add("Vilosell", 0.04);

            // TARRAGONA
            Add("Aiguamurcia", 0.04); Add("Albinyana", 0.04); Add("Albiol", 0.04); Add("Alcover", 0.04);
            Add("Aldea", 0.04); Add("Aldover", 0.04); Add("Aleixar", 0.04); Add("Alforja", 0.04);
            Add("Alio", 0.04); Add("Altafulla", 0.04); Add("Ametlla de Mar", 0.04); Add("Ampolla", 0.04);
            Add("Amposta", 0.04); Add("Arboc", 0.04); Add("Arboli", 0.04); Add("Argentera", 0.04);
            Add("Asco", 0.04); Add("Banyeres del Penedes", 0.04); Add("Barbera de la Conca", 0.04); Add("Bellmunt del Priorat", 0.04);
            Add("Bellvei", 0.04); Add("Benifallet", 0.04); Add("Benissanet", 0.04); Add("Bisbal de Falset", 0.04);
            Add("Bisbal del Penedes", 0.04); Add("Blancafort", 0.04); Add("Bonastre", 0.04); Add("Borges del Camp", 0.04);
            Add("Botarell", 0.04); Add("Brafim", 0.04); Add("Cabaces", 0.04); Add("Cabra del Camp", 0.04);
            Add("Calafell", 0.04); Add("Camarles", 0.04); Add("Cambrils", 0.04); Add("Capafonts", 0.04);
            Add("Capcanes", 0.04); Add("Castellvell del Camp", 0.04); Add("Catllar", 0.04); Add("Colldejou", 0.04);
            Add("Conesa", 0.04); Add("Constanti", 0.04); Add("Corbera d'Ebre", 0.04); Add("Cornudella de Montsant", 0.04);
            Add("Creixell", 0.04); Add("Cunit", 0.04); Add("Deltebre", 0.04); Add("Duesaigues", 0.04);
            Add("Espluga de Francoli", 0.04); Add("Falset", 0.04); Add("Fatarella", 0.04); Add("Febro", 0.04);
            Add("Figuera", 0.04); Add("Figuerola del Camp", 0.04); Add("Flix", 0.04); Add("Fores", 0.04);
            Add("Freginals", 0.04); Add("Garcia", 0.04); Add("Garidells", 0.04); Add("Ginestar", 0.04);
            Add("Gratallops", 0.04); Add("Guiamets", 0.04); Add("Lloar", 0.04); Add("Llorenc del Penedes", 0.04);
            Add("Marca", 0.04); Add("Margalef", 0.04); Add("Masdenverge", 0.04); Add("Masllorenc", 0.04);
            Add("Maso", 0.04); Add("Maspujols", 0.04); Add("Masroig", 0.04); Add("Mila", 0.04);
            Add("Miravet", 0.04); Add("Molar", 0.04); Add("Montblanc", 0.04); Add("Montbrio del Camp", 0.04);
            Add("Montferri", 0.04); Add("Montmell", 0.04); Add("Mont-ral", 0.04); Add("Mont-roig del Camp", 0.04);
            Add("Mora d'Ebre", 0.04); Add("Mora la Nova", 0.04); Add("Morell", 0.04); Add("Morera de Montsant", 0.04);
            Add("Nou de Gaia", 0.04); Add("Nulles", 0.04); Add("Pallaresos", 0.04); Add("Palma d'Ebre", 0.04);
            Add("Perafort", 0.04); Add("Perello", 0.04); Add("Piles", 0.04); Add("Pinell de Brai", 0.04);
            Add("Pira", 0.04); Add("Pla de Santa Maria", 0.04); Add("Pobla de Mafumet", 0.04); Add("Pobla de Montornes", 0.04);
            Add("Poboleda", 0.04); Add("Pont d'Armentera", 0.04); Add("Pontils", 0.04); Add("Porrera", 0.04);
            Add("Pradell de la Teixeta", 0.04); Add("Prades", 0.04); Add("Pratdip", 0.04); Add("Puigpelat", 0.04);
            Add("Querol", 0.04); Add("Rasquera", 0.04); Add("Renau", 0.04); Add("Reus", 0.04);
            Add("Riba", 0.04); Add("Riera de Gaia", 0.04); Add("Riudecanyes", 0.04); Add("Riudecols", 0.04);
            Add("Riudoms", 0.04); Add("Rocafort de Queralt", 0.04); Add("Roda de Bara", 0.04); Add("Rodonya", 0.04);
            Add("Roquetes", 0.04); Add("Rourell", 0.04); Add("Salomo", 0.04); Add("Salou", 0.04);
            Add("Sant Carles de la Rapita", 0.04); Add("Sant Jaume dels Domenys", 0.04); Add("Sant Jaume d'Enveja", 0.04); Add("Santa Barbara", 0.04);
            Add("Santa Coloma de Queralt", 0.04); Add("Santa Oliva", 0.04); Add("Sarral", 0.04); Add("Secuita", 0.04);
            Add("Selva del Camp", 0.04); Add("Solivella", 0.04); Add("Tarragona", 0.04); Add("Tivenys", 0.04);
            Add("Tivissa", 0.04); Add("Torre de Fontaubella", 0.04); Add("Torre de l'Espanyol", 0.04); Add("Torredembarra", 0.04);
            Add("Torroja del Priorat", 0.04); Add("Tortosa", 0.04); Add("Ulldemolins", 0.04); Add("Vallclara", 0.04);
            Add("Vallmoll", 0.04); Add("Valls", 0.04); Add("Vandellos i l'Hospitalet de l'Infant", 0.04); Add("Vendrell", 0.04);
            Add("Vespella de Gaia", 0.04); Add("Vilabella", 0.04); Add("Vilallonga del Camp", 0.04); Add("Vilanova de Prades", 0.04);
            Add("Vilanova d'Escornalbou", 0.04); Add("Vilaplana", 0.04); Add("Vila-rodona", 0.04); Add("Vila-seca", 0.04);
            Add("Vilaverd", 0.04); Add("Vilella Alta", 0.04); Add("Vilella Baixa", 0.04); Add("Vimbodi", 0.04);
            Add("Vinebre", 0.04); Add("Vinyols i els Arcs", 0.04); Add("Xerta", 0.04);

            // =================================================================
            // COMUNIDAD VALENCIANA
            // =================================================================
            // ALICANTE
            Add("Adsubia", 0.07); Add("Agost", 0.11); Add("Agres", 0.07); Add("Aigues", 0.11);
            Add("Albatera", 0.15); Add("Alcalali", 0.07); Add("Alcocer de Planes", 0.07); Add("Alcoleja", 0.08);
            Add("Alcoy", 0.07); Add("Alfafara", 0.07); Add("Alfas del Pi", 0.08); Add("Algorfa", 0.16);
            Add("Alguena", 0.12); Add("Alicante", 0.14); Add("Almoradi", 0.16); Add("Almudaina", 0.07);
            Add("Alqueria d'Asnar", 0.07); Add("Altea", 0.08); Add("Aspe", 0.13); Add("Balones", 0.07);
            Add("Banyeres de Mariola", 0.07); Add("Benasau", 0.07); Add("Beneixama", 0.07); Add("Benejuzar", 0.16);
            Add("Benferri", 0.15); Add("Beniarbeig", 0.07); Add("Beniarda", 0.07); Add("Beniarres", 0.07);
            Add("Benidoleig", 0.07); Add("Benidorm", 0.09); Add("Benifallim", 0.08); Add("Benifato", 0.08);
            Add("Benigembla", 0.07); Add("Benijofar", 0.15); Add("Benilloba", 0.07); Add("Benillup", 0.07);
            Add("Benimantell", 0.08); Add("Benimarfull", 0.07); Add("Benimassot", 0.07); Add("Benimeli", 0.07);
            Add("Benissa", 0.06); Add("Benitachell", 0.05); Add("Biar", 0.07); Add("Bigastro", 0.16);
            Add("Bolulla", 0.07); Add("Busot", 0.11); Add("Callosa de Ensarria", 0.08); Add("Callosa de Segura", 0.16);
            Add("Calpe", 0.06); Add("Campello", 0.13); Add("Campo de Mirra", 0.07); Add("Canada", 0.07);
            Add("Castalla", 0.08); Add("Castell de Castells", 0.07); Add("Catral", 0.15); Add("Cocentaina", 0.07);
            Add("Confrides", 0.08); Add("Cox", 0.16); Add("Crevillent", 0.15); Add("Daya Nueva", 0.16);
            Add("Daya Vieja", 0.16); Add("Denia", 0.06); Add("Dolores", 0.16); Add("Elche", 0.15);
            Add("Elda", 0.09); Add("Facheca", 0.07); Add("Famorca", 0.07); Add("Finestrat", 0.09);
            Add("Formentera del Segura", 0.15); Add("Gaianes", 0.07); Add("Gata de Gorgos", 0.06); Add("Gorga", 0.07);
            Add("Granja de Rocamora", 0.15); Add("Guadalest", 0.07); Add("Guardamar del Segura", 0.15); Add("Hondon de las Nieves", 0.13);
            Add("Hondon de los Frailes", 0.14); Add("Ibi", 0.08); Add("Jacarilla", 0.16); Add("Jalon", 0.07);
            Add("Javea", 0.05); Add("Jijona", 0.09); Add("Lorcha", 0.07); Add("Lliber", 0.07);
            Add("Millena", 0.07); Add("Monforte del Cid", 0.12); Add("Monovar", 0.10); Add("Montesinos", 0.15);
            Add("Murla", 0.07); Add("Muro de Alcoy", 0.07); Add("Mutxamel", 0.13); Add("Novelda", 0.12);
            Add("Nucia", 0.08); Add("Ondara", 0.06); Add("Onil", 0.07); Add("Orba", 0.07);
            Add("Orihuela", 0.16); Add("Orxeta", 0.09); Add("Parcent", 0.07); Add("Pedreguer", 0.06);
            Add("Pego", 0.07); Add("Penaguila", 0.07); Add("Petrer", 0.09); Add("Pilar de la Horadada", 0.12);
            Add("Pinoso", 0.09); Add("Planes", 0.07); Add("Poblets", 0.06); Add("Polop", 0.08);
            Add("Quatretondeta", 0.07); Add("Rafal", 0.16); Add("Rafol d'Almunia", 0.07); Add("Redovan", 0.16);
            Add("Relleu", 0.08); Add("Rojales", 0.15); Add("Romana", 0.11); Add("Sagra", 0.07);
            Add("Salinas", 0.08); Add("San Fulgencio", 0.16); Add("San Isidro", 0.15); Add("San Miguel de Salinas", 0.15);
            Add("San Vicente del Raspeig", 0.13); Add("Sanet y Negrals", 0.07); Add("Sant Joan d'Alacant", 0.13); Add("Santa Pola", 0.15);
            Add("Sax", 0.08); Add("Sella", 0.08); Add("Senija", 0.06); Add("Tarbena", 0.07);
            Add("Teulada", 0.06); Add("Tibi", 0.09); Add("Tollos", 0.07); Add("Tormos", 0.07);
            Add("Torremanzanas", 0.08); Add("Torrevieja", 0.14); Add("Vall de Ebo", 0.07); Add("Vall de Gallinera", 0.07);
            Add("Vall de Laguart", 0.07); Add("Verger", 0.06); Add("Villajoyosa", 0.10); Add("Villena", 0.07);

            // VALENCIA
            Add("Ademuz", 0.05); Add("Ador", 0.07); Add("Agullent", 0.07); Add("Aielo de Malferit", 0.07);
            Add("Aielo de Rugat", 0.07); Add("Alaquas", 0.06); Add("Albaida", 0.07); Add("Albal", 0.07);
            Add("Albalat de la Ribera", 0.07); Add("Albalat dels Sorells", 0.06); Add("Albalat dels Tarongers", 0.04); Add("Alberic", 0.07);
            Add("Alborache", 0.06); Add("Alboraya", 0.06); Add("Albuixech", 0.06); Add("Alcantera de Xuquer", 0.07);
            Add("Alcasser", 0.07); Add("Alcudia", 0.07); Add("Alcudia de Crespins", 0.07); Add("Aldaia", 0.07);
            Add("Alfafar", 0.07); Add("Alfara del Patriarca", 0.06); Add("Alfarp", 0.07); Add("Alfarrasi", 0.07);
            Add("Alfauir", 0.07); Add("Algemesi", 0.07); Add("Algimia de Alfara", 0.04); Add("Alginet", 0.07);
            Add("Almassera", 0.06); Add("Almisera", 0.07); Add("Almoines", 0.07); Add("Almussafes", 0.07);
            Add("Alqueria de la Comtessa", 0.07); Add("Alzira", 0.07); Add("Anna", 0.07); Add("Antella", 0.07);
            Add("Atzeneta d'Albaida", 0.07); Add("Ayora", 0.07); Add("Barx", 0.07); Add("Barxeta", 0.07);
            Add("Belgida", 0.07); Add("Bellreguard", 0.07); Add("Bellus", 0.07); Add("Benaguasil", 0.05);
            Add("Beneixida", 0.07); Add("Benetusser", 0.07); Add("Beniarjo", 0.07); Add("Beniatjar", 0.07);
            Add("Benicolet", 0.07); Add("Benifaio", 0.07); Add("Benifairo de la Valldigna", 0.07); Add("Benifairo de les Valls", 0.04);
            Add("Beniganim", 0.07); Add("Benimodo", 0.07); Add("Benimuslem", 0.07); Add("Beniparrell", 0.07);
            Add("Benirredra", 0.07); Add("Benisano", 0.05); Add("Benissoda", 0.07); Add("Benisuera", 0.07);
            Add("Betera", 0.05); Add("Bicorp", 0.07); Add("Bocairent", 0.07); Add("Bolbaite", 0.07);
            Add("Bonrepos i Mirambell", 0.06); Add("Bufali", 0.07); Add("Bugarra", 0.04); Add("Bunol", 0.06);
            Add("Burjassot", 0.06); Add("Canals", 0.07); Add("Canet d'En Berenguer", 0.04); Add("Carcaixent", 0.07);
            Add("Carcer", 0.07); Add("Carlet", 0.07); Add("Carricola", 0.07); Add("Castello de Rugat", 0.07);
            Add("Castellonet de la Conquesta", 0.07); Add("Castielfabib", 0.05); Add("Catadau", 0.07); Add("Catarroja", 0.07);
            Add("Cerdà", 0.07); Add("Chella", 0.07); Add("Cheste", 0.06); Add("Chiva", 0.06);
            Add("Cofrentes", 0.07); Add("Corbera", 0.07); Add("Cortes de Pallas", 0.06); Add("Cotes", 0.07);
            Add("Cullera", 0.07); Add("Daimus", 0.07); Add("Dos Aguas", 0.06); Add("Eliana", 0.05);
            Add("Emperador", 0.06); Add("Enguera", 0.07); Add("Enova", 0.07); Add("Estivella", 0.04);
            Add("Estubeny", 0.07); Add("Faura", 0.04); Add("Favara", 0.07); Add("Foios", 0.06);
            Add("Font de la Figuera", 0.07); Add("Font d'En Carros", 0.07); Add("Fontanars dels Alforins", 0.07); Add("Fortaleny", 0.07);
            Add("Gandia", 0.07); Add("Gavarda", 0.07); Add("Genoves", 0.07); Add("Gilet", 0.04);
            Add("Godella", 0.06); Add("Godelleta", 0.06); Add("Granja de la Costera", 0.07); Add("Guadassuar", 0.07);
            Add("Guardamar de la Safor", 0.07); Add("Jalance", 0.07); Add("Jarafuel", 0.07); Add("Llanera de Ranes", 0.07);
            Add("Llauri", 0.07); Add("Lliria", 0.05); Add("Llocnou de la Corona", 0.07); Add("Llocnou de Sant Jeroni", 0.07);
            Add("Llocnou d'En Fenollet", 0.07); Add("Llombai", 0.07); Add("Llosa de Ranes", 0.07); Add("Llutxent", 0.07);
            Add("Loriguilla", 0.05); Add("Losa del Obispo", 0.04); Add("Macastre", 0.06); Add("Manises", 0.06);
            Add("Manuel", 0.07); Add("Massalaves", 0.07); Add("Massalfassar", 0.06); Add("Massamagrell", 0.06);
            Add("Massanassa", 0.07); Add("Meliana", 0.06); Add("Miramar", 0.07); Add("Mislata", 0.06);
            Add("Mogente", 0.07); Add("Moncada", 0.06); Add("Monserrat", 0.07); Add("Montaverner", 0.07);
            Add("Montesa", 0.07); Add("Montichelvo", 0.07); Add("Montroy", 0.07); Add("Museros", 0.06);
            Add("Naquera", 0.05); Add("Navarres", 0.07); Add("Novele", 0.07); Add("Oliva", 0.07);
            Add("Olleria", 0.07); Add("Olocau", 0.04); Add("Ontinyent", 0.07); Add("Otos", 0.07);
            Add("Paiporta", 0.07); Add("Palma de Gandia", 0.07); Add("Palmera", 0.07); Add("Palomar", 0.07);
            Add("Paterna", 0.06); Add("Pedralba", 0.04); Add("Petres", 0.04); Add("Picanya", 0.07);
            Add("Picassent", 0.07); Add("Piles", 0.07); Add("Pinet", 0.07); Add("Pobla de Farnals", 0.06);
            Add("Pobla de Vallbona", 0.05); Add("Pobla del Duc", 0.07); Add("Pobla Llarga", 0.07); Add("Polinya de Xuquer", 0.07);
            Add("Potries", 0.07); Add("Pucol", 0.05); Add("Puig", 0.05); Add("Quart de Poblet", 0.06);
            Add("Quart de les Valls", 0.04); Add("Quartell", 0.04); Add("Quatretonda", 0.07); Add("Quesa", 0.07);
            Add("Rafelbunyol", 0.06); Add("Rafelcofer", 0.07); Add("Rafelguaraf", 0.07); Add("Rafol de Salem", 0.07);
            Add("Real de Gandia", 0.07); Add("Real de Montroi", 0.07); Add("Requena", 0.05); Add("Riba-roja de Turia", 0.06);
            Add("Riola", 0.07); Add("Rocafort", 0.06); Add("Rotgla i Corbera", 0.07); Add("Rotova", 0.07);
            Add("Rugat", 0.07); Add("Sagunto", 0.04); Add("Salem", 0.07); Add("San Antonio de Benageber", 0.06);
            Add("San Juan de Enova", 0.07); Add("Sedavi", 0.07); Add("Segart", 0.05); Add("Sellent", 0.07);
            Add("Sempere", 0.07); Add("Senyera", 0.07); Add("Serra", 0.05); Add("Siete Aguas", 0.04);
            Add("Silla", 0.07); Add("Simat de la Valldigna", 0.07); Add("Sollana", 0.07); Add("Sueca", 0.07);
            Add("Sumacarcer", 0.07); Add("Tavernes Blanques", 0.06); Add("Tavernes de la Valldigna", 0.07); Add("Teresa de Cofrentes", 0.07);
            Add("Terrateig", 0.07); Add("Torrella", 0.07); Add("Torrent", 0.07); Add("Torres Torres", 0.04);
            Add("Tous", 0.07); Add("Turis", 0.06); Add("Valencia", 0.06); Add("Vallada", 0.07);
            Add("Valles", 0.07); Add("Vilamarxant", 0.05); Add("Villalonga", 0.07); Add("Villanueva de Castellon", 0.07);
            Add("Vinalesa", 0.06); Add("Xativa", 0.07); Add("Xeraco", 0.07); Add("Xeresa", 0.07);
            Add("Xirivella", 0.07); Add("Yatova", 0.06); Add("Zarra", 0.07);

            // =================================================================
            // EXTREMADURA
            // =================================================================
            // BADAJOZ
            Add("Aceuchal", 0.04, 1.3); Add("Albuera", 0.05, 1.3); Add("Alburquerque", 0.04, 1.3); Add("Alconchel", 0.06, 1.3);
            Add("Alconera", 0.04, 1.3); Add("Almendral", 0.05, 1.3); Add("Atalaya", 0.05, 1.3); Add("Badajoz", 0.05, 1.3);
            Add("Barcarrota", 0.05, 1.3); Add("Bienvenida", 0.04, 1.3); Add("Bodonal de la Sierra", 0.05, 1.3);
            Add("Burguillos del Cerro", 0.05, 1.3); Add("Cabeza la Vaca", 0.05, 1.3); Add("Calera de Leon", 0.05, 1.3);
            Add("Calzadilla de los Barros", 0.04, 1.3); Add("Casas de Reina", 0.04, 1.2); Add("Codosera", 0.04, 1.3);
            Add("Corte de Peleas", 0.04, 1.3); Add("Cheles", 0.07, 1.2); Add("Entrin Bajo", 0.04, 1.3); Add("Feria", 0.04, 1.3);
            Add("Fregenal de la Sierra", 0.05, 1.3); Add("Fuente de Cantos", 0.04, 1.3); Add("Fuente del Arco", 0.04, 1.2);
            Add("Fuente del Maestre", 0.04, 1.3); Add("Fuentes de Leon", 0.05, 1.3); Add("Higuera de Vargas", 0.06, 1.3);
            Add("Higuera la Real", 0.06, 1.3); Add("Jerez de los Caballeros", 0.05, 1.3); Add("Lapa", 0.04, 1.3);
            Add("Llerena", 0.04, 1.3); Add("Lobon", 0.04, 1.3); Add("Malcocinado", 0.04, 1.2); Add("Medina de las Torres", 0.04, 1.3);
            Add("Monesterio", 0.04, 1.3); Add("Montemolin", 0.04, 1.3); Add("Morera", 0.05, 1.3); Add("Nogales", 0.05, 1.3);
            Add("Oliva de la Frontera", 0.06, 1.3); Add("Olivenza", 0.05, 1.3); Add("Parra", 0.05, 1.3);
            Add("Puebla de la Calzada", 0.04, 1.3); Add("Puebla de Sancho Perez", 0.04, 1.3); Add("Puebla del Maestre", 0.04, 1.2);
            Add("Pueblonuevo del Guadiana", 0.05, 1.3); Add("Reina", 0.04, 1.2); Add("Roca de la Sierra", 0.05, 1.3);
            Add("Salvaleon", 0.05, 1.3); Add("Salvatierra de los Barros", 0.05, 1.3); Add("San Vicente de Alcantara", 0.04, 1.2);
            Add("Santa Marta", 0.04, 1.3); Add("Santos de Maimona", 0.04, 1.3); Add("Segura de Leon", 0.05, 1.3);
            Add("Solana de los Barros", 0.04, 1.3); Add("Talavera la Real", 0.04, 1.3); Add("Taliga", 0.06, 1.3);
            Add("Torre de Miguel Sesmero", 0.05, 1.3); Add("Trasierra", 0.04, 1.2); Add("Usagre", 0.04, 1.3);
            Add("Valdelacalzada", 0.04, 1.3); Add("Valencia del Mombuey", 0.08, 1.2); Add("Valencia del Ventoso", 0.05, 1.3);
            Add("Valle de Matamoros", 0.05, 1.3); Add("Valle de Santa Ana", 0.05, 1.3); Add("Valverde de Burguillos", 0.05, 1.3);
            Add("Valverde de Leganes", 0.05, 1.3); Add("Villafranca de los Barros", 0.04, 1.3); Add("Villagarcia de la Torre", 0.04, 1.3);
            Add("Villalba de los Barros", 0.04, 1.3); Add("Villanueva del Fresno", 0.07, 1.2); Add("Villar del Rey", 0.05, 1.3);
            Add("Zafra", 0.04, 1.3); Add("Zahinos", 0.06, 1.3);

            // CÁCERES
            Add("Carbajo", 0.04, 1.2); Add("Cedillo", 0.07, 1.1); Add("Herrera de Alcantara", 0.06, 1.1); Add("Membrio", 0.04, 1.2);
            Add("Salorino", 0.04, 1.2); Add("Santiago de Alcantara", 0.04, 1.2); Add("Valencia de Alcantara", 0.04, 1.2);

            // =================================================================
            // GALICIA
            // =================================================================
            // A CORUÑA
            Add("Melide", 0.04); Add("Santiso", 0.04); Add("Toques", 0.04);

            // LUGO
            Add("Abadin", 0.04); Add("Alfoz", 0.04); Add("Antas de Ulla", 0.04); Add("Baleira", 0.04); Add("Baralla", 0.04);
            Add("Barreiros", 0.04); Add("Becerrea", 0.04); Add("Begonte", 0.04); Add("Boveda", 0.04); Add("Carballedo", 0.04);
            Add("Castro de Rei", 0.04); Add("Castroverde", 0.04); Add("Cervantes", 0.04); Add("Chantada", 0.04); Add("Corgo", 0.04);
            Add("Cospeito", 0.04); Add("Folgoso do Courel", 0.04); Add("Fonsagrada", 0.04); Add("Foz", 0.04); Add("Friol", 0.04);
            Add("Guitiriz", 0.04); Add("Guntin", 0.04); Add("Incio", 0.04); Add("Lancara", 0.04); Add("Lourenza", 0.04);
            Add("Lugo", 0.04); Add("Meira", 0.04); Add("Mondonedo", 0.04); Add("Monforte de Lemos", 0.04); Add("Monterroso", 0.04);
            Add("Muras", 0.04); Add("Navia de Suarna", 0.04); Add("Nogais", 0.04); Add("Ourol", 0.04); Add("Outeiro de Rei", 0.04);
            Add("Palas de Rei", 0.04); Add("Panton", 0.04); Add("Paradela", 0.04); Add("Paramo", 0.04); Add("Pastoriza", 0.04);
            Add("Pedrafita do Cebreiro", 0.04); Add("Pobra do Brollon", 0.04); Add("Pol", 0.04); Add("Pontenova", 0.04);
            Add("Portomarin", 0.04); Add("Quiroga", 0.04); Add("Rabade", 0.04); Add("Ribas de Sil", 0.04); Add("Ribeira de Piquin", 0.04);
            Add("Riotorto", 0.04); Add("Samos", 0.04); Add("Sarria", 0.04); Add("Savinao", 0.04); Add("Sober", 0.04);
            Add("Taboada", 0.04); Add("Trabada", 0.04); Add("Triacastela", 0.04); Add("Valadouro", 0.04); Add("Vicedo", 0.04);
            Add("Vilalba", 0.04); Add("Viveiro", 0.04); Add("Xermade", 0.04); Add("Xove", 0.04);

            // OURENSE
            Add("Allariz", 0.04); Add("Amoeiro", 0.04); Add("Arnoia", 0.04); Add("Avion", 0.04); Add("Baltar", 0.04); Add("Bande", 0.04);
            Add("Banos de Molgas", 0.04); Add("Barbadas", 0.04); Add("Beade", 0.04); Add("Beariz", 0.04); Add("Blancos", 0.04);
            Add("Boboras", 0.04); Add("Bola", 0.04); Add("Bolo", 0.04); Add("Calvos de Randin", 0.04); Add("Carballeda de Avia", 0.04);
            Add("Carballino", 0.04); Add("Cartelle", 0.04); Add("Castrelo de Mino", 0.04); Add("Castrelo do Val", 0.04);
            Add("Castro Caldelas", 0.04); Add("Celanova", 0.04); Add("Cenlle", 0.04); Add("Coles", 0.04); Add("Cortegada", 0.04);
            Add("Cualedro", 0.04); Add("Chandrexa de Queixa", 0.04); Add("Entrimo", 0.04); Add("Esgos", 0.04); Add("Gomesende", 0.04);
            Add("Irixo", 0.04); Add("Larouco", 0.04); Add("Laza", 0.04); Add("Leiro", 0.04); Add("Lobeira", 0.04); Add("Lobios", 0.04);
            Add("Maceda", 0.04); Add("Manzaneda", 0.04); Add("Maside", 0.04); Add("Melon", 0.04); Add("Merca", 0.04);
            Add("Montederramo", 0.04); Add("Monterrei", 0.04); Add("Muinos", 0.04); Add("Nogueira de Ramuin", 0.04); Add("Oimbra", 0.04);
            Add("Ourense", 0.04); Add("Paderne de Allariz", 0.04); Add("Padrenda", 0.04); Add("Parada de Sil", 0.04);
            Add("Pereiro de Aguiar", 0.04); Add("Peroxa", 0.04); Add("Petin", 0.04); Add("Pinor", 0.04); Add("Pobra de Trives", 0.04);
            Add("Pontedeva", 0.04); Add("Porqueira", 0.04); Add("Punxin", 0.04); Add("Quintela de Leirado", 0.04);
            Add("Rairiz de Veiga", 0.04); Add("Ramiras", 0.04); Add("Ribadavia", 0.04); Add("Rua", 0.04); Add("San Amaro", 0.04);
            Add("San Cibrao das Vinas", 0.04); Add("San Cristovo de Cea", 0.04); Add("San Xoan de Rio", 0.04); Add("Sandias", 0.04);
            Add("Sarreaus", 0.04); Add("Taboadela", 0.04); Add("Teixeira", 0.04); Add("Toen", 0.04); Add("Trasmiras", 0.04);
            Add("Verea", 0.04); Add("Verin", 0.04); Add("Viana do Bolo", 0.04); Add("Vilamarin", 0.04);
            Add("Vilamartin de Valdeorras", 0.04); Add("Vilar de Barrio", 0.04); Add("Vilar de Santos", 0.04);
            Add("Vilarino de Conso", 0.04); Add("Xinzo de Limia", 0.04); Add("Xunqueira de Ambia", 0.04);
            Add("Xunqueira de Espadanedo", 0.04);

            // PONTEVEDRA
            Add("Agolada", 0.04); Add("Arbo", 0.04); Add("Caniza", 0.04); Add("Covelo", 0.04); Add("Crecente", 0.04); Add("Dozon", 0.04);
            Add("Forcarei", 0.04); Add("Igrexa", 0.04); Add("Lalin", 0.04); Add("Mondariz", 0.04); Add("Mondariz-Balneario", 0.04);
            Add("Neves", 0.04); Add("Pedreira", 0.04); Add("Rodeiro", 0.04); Add("Silleda", 0.04); Add("Vila de Cruces", 0.04);

            // =================================================================
            // ILLES BALEARS
            // =================================================================
            Add("Alaior", 0.04); Add("Alaro", 0.04); Add("Alcudia", 0.04); Add("Algaida", 0.04); Add("Andratx", 0.04);
            Add("Ariany", 0.04); Add("Arta", 0.04); Add("Banyalbufar", 0.04); Add("Binissalem", 0.04); Add("Buger", 0.04);
            Add("Bunyola", 0.04); Add("Calvia", 0.04); Add("Campanet", 0.04); Add("Campos", 0.04); Add("Capdepera", 0.04);
            Add("Castell", 0.04); Add("Ciutadella de Menorca", 0.04); Add("Consell", 0.04); Add("Costitx", 0.04); Add("Deya", 0.04);
            Add("Eivissa", 0.04); Add("Escorca", 0.04); Add("Esporles", 0.04); Add("Estellencs", 0.04); Add("Felanitx", 0.04);
            Add("Ferreries", 0.04); Add("Formentera", 0.04); Add("Fornalutx", 0.04); Add("Inca", 0.04); Add("Lloret de Vistalegre", 0.04);
            Add("Lloseta", 0.04); Add("Llubi", 0.04); Add("Llucmajor", 0.04); Add("Mahon", 0.04); Add("Manacor", 0.04);
            Add("Mancor de la Vall", 0.04); Add("Maria de la Salut", 0.04); Add("Marratxi", 0.04); Add("Mercadal", 0.04);
            Add("Migjorn Gran", 0.04); Add("Montuiri", 0.04); Add("Muro", 0.04); Add("Palma de Mallorca", 0.04); Add("Petra", 0.04);
            Add("Pobla", 0.04); Add("Pollenca", 0.04); Add("Porreres", 0.04); Add("Puigpunyent", 0.04); Add("Salines", 0.04);
            Add("San Jose", 0.04); Add("Sant Antoni de Portmany", 0.04); Add("Sant Joan", 0.04); Add("Sant Joan de Labritja", 0.04);
            Add("Sant Llorenc des Cardassar", 0.04); Add("Sant Lluis", 0.04); Add("Santa Eugenia", 0.04);
            Add("Santa Eulalia del Rio", 0.04); Add("Santa Margalida", 0.04); Add("Santa Maria del Cami", 0.04); Add("Santanyi", 0.04);
            Add("Selva", 0.04); Add("Sencelles", 0.04); Add("Sineu", 0.04); Add("Soller", 0.04); Add("Son Servera", 0.04);
            Add("Valldemosa", 0.04); Add("Vilafranca de Bonany", 0.04);

            // =================================================================
            // REGIÓN DE MURCIA
            // =================================================================
            Add("Abanilla", 0.15); Add("Abaran", 0.10); Add("Aguilas", 0.09); Add("Albudeite", 0.11); Add("Alcantarilla", 0.15);
            Add("Alcazares", 0.08); Add("Aledo", 0.10); Add("Alguazas", 0.14); Add("Alhama de Murcia", 0.11); Add("Archena", 0.13);
            Add("Beniel", 0.16); Add("Blanca", 0.11); Add("Bullas", 0.08); Add("Calasparra", 0.07); Add("Campos del Rio", 0.12);
            Add("Caravaca de la Cruz", 0.07); Add("Cartagena", 0.07); Add("Cehegin", 0.08); Add("Ceuti", 0.14); Add("Cieza", 0.09);
            Add("Fortuna", 0.15); Add("Fuente Alamo de Murcia", 0.11); Add("Jumilla", 0.07); Add("Librilla", 0.12); Add("Lorca", 0.12);
            Add("Lorqui", 0.14); Add("Mazarron", 0.09); Add("Molina de Segura", 0.15); Add("Moratalla", 0.07); Add("Mula", 0.09);
            Add("Murcia", 0.15); Add("Ojos", 0.12); Add("Pliego", 0.09); Add("Puerto Lumbreras", 0.14); Add("Ricote", 0.12);
            Add("San Javier", 0.10); Add("San Pedro del Pinatar", 0.11); Add("Santomera", 0.16); Add("Torre Pacheco", 0.09);
            Add("Torres de Cotillas", 0.14); Add("Totana", 0.10); Add("Ulea", 0.12); Add("Union", 0.07);
            Add("Villanueva del Rio Segura", 0.13); Add("Yecla", 0.07);

            // =================================================================
            // COMUNIDAD FORAL DE NAVARRA
            // =================================================================
            Add("Amescoa Baja", 0.04); Add("Ancin", 0.04); Add("Ansoain", 0.04); Add("Anue", 0.04); Add("Anorbe", 0.04);
            Add("Aoiz", 0.05); Add("Araitz", 0.04); Add("Arakil", 0.04); Add("Aranarache", 0.04); Add("Aranguren", 0.04);
            Add("Arano", 0.04); Add("Arantza", 0.04); Add("Arbizu", 0.04); Add("Arce", 0.05); Add("Arellano", 0.04); Add("Areso", 0.04);
            Add("Aria", 0.05); Add("Aribe", 0.05); Add("Arruazu", 0.04); Add("Artajona", 0.04); Add("Artazu", 0.04); Add("Atez", 0.04);
            Add("Auritz", 0.05); Add("Ayegui", 0.04); Add("Bakaiku", 0.04); Add("Baranain", 0.04); Add("Barasoain", 0.04);
            Add("Barbarin", 0.04); Add("Basaburua", 0.04); Add("Baztan", 0.05); Add("Beintza-Labaien", 0.04); Add("Belascoain", 0.04);
            Add("Bera", 0.04); Add("Berrioplano", 0.04); Add("Berriozar", 0.04); Add("Bertizarana", 0.04); Add("Betelu", 0.04);
            Add("Bidaurreta", 0.04); Add("Biurrun-Olcoz", 0.04); Add("Burgui", 0.05); Add("Burlada", 0.04); Add("Castillonuevo", 0.05);
            Add("Cirauqui", 0.04); Add("Ciriza", 0.04); Add("Cizur", 0.04); Add("Dicastillo", 0.04); Add("Donamaria", 0.04);
            Add("Doneztebe", 0.04); Add("Echarri", 0.04); Add("Egues", 0.04); Add("Elgorriaga", 0.04); Add("Eneriz", 0.04);
            Add("Eratsun", 0.04); Add("Ergoiena", 0.04); Add("Erro", 0.05); Add("Esteribar", 0.04); Add("Etayo", 0.04);
            Add("Etxalar", 0.04); Add("Etxarri-Aranatz", 0.04); Add("Etzauri", 0.04); Add("Eulate", 0.04); Add("Ezcabarte", 0.04);
            Add("Ezcaroz", 0.05); Add("Ezkurra", 0.04); Add("Galar", 0.04); Add("Gallues", 0.05); Add("Garaioa", 0.05);
            Add("Garde", 0.05); Add("Garralda", 0.05); Add("Goni", 0.04); Add("Guesalaz", 0.04); Add("Guirguillano", 0.04);
            Add("Hiriberri", 0.05); Add("Huarte", 0.04); Add("Ibargoiti", 0.04); Add("Igantzi", 0.04); Add("Imotz", 0.04);
            Add("Iraneta", 0.04); Add("Irurtzun", 0.04); Add("Isaba", 0.05); Add("Ituren", 0.04); Add("Iturmendi", 0.04);
            Add("Iza", 0.04); Add("Izalzu", 0.05); Add("Jaurrieta", 0.05); Add("Juslapena", 0.04); Add("Lakuntza", 0.04);
            Add("Larraun", 0.04); Add("Larrona", 0.04); Add("Leitz", 0.04); Add("Lekunberri", 0.04); Add("Lesaka", 0.04);
            Add("Lezaun", 0.04); Add("Lizoain", 0.04); Add("Longuida", 0.05); Add("Luzaide", 0.05); Add("Maneru", 0.04);
            Add("Metauten", 0.04); Add("Monreal", 0.04); Add("Navasues", 0.05); Add("Obanos", 0.04); Add("Ochagavia", 0.05);
            Add("Odieta", 0.04); Add("Oiz", 0.04); Add("Olaibar", 0.04); Add("Olazti", 0.04); Add("Ollaran", 0.04);
            Add("Orbaizeta", 0.05); Add("Orbara", 0.05); Add("Oronz", 0.05); Add("Oroz-Betelu", 0.05); Add("Pamplona", 0.04);
            Add("Puente la Reina", 0.04); Add("Romanzado", 0.05); Add("Roncal", 0.05); Add("Roncesvalles", 0.05); Add("Saldias", 0.04);
            Add("Salinas de Oro", 0.04); Add("Sunbilla", 0.04); Add("Tiebas-Muruarte de Reta", 0.04); Add("Ucar", 0.04);
            Add("Uharte-Arakil", 0.04); Add("Ultzama", 0.04); Add("Unciti", 0.04); Add("Urdax", 0.04); Add("Urdiain", 0.04);
            Add("Urraul Alto", 0.05); Add("Urraul Bajo", 0.05); Add("Urrotz", 0.04); Add("Urzainqui", 0.05); Add("Uterga", 0.04);
            Add("Uztarroz", 0.05); Add("Valcarlos", 0.05); Add("Vera de Bidasoa", 0.04); Add("Vidaurreta", 0.04); Add("Villava", 0.04);
            Add("Yerri", 0.04); Add("Zabalza", 0.04); Add("Ziordia", 0.04); Add("Zizur Mayor", 0.04); Add("Zubieta", 0.04);
            Add("Zugarramurdi", 0.04);

            // =================================================================
            // PAÍS VASCO
            // =================================================================
            // ÁLAVA
            Add("Alegría-Dulantzi", 0.04); Add("Amurrio", 0.04); Add("Añana", 0.04); Add("Aramaio", 0.05); Add("Armiñón", 0.04);
            Add("Arraia-Maeztu", 0.04); Add("Arratzua-Ubarrundia", 0.04); Add("Artziniega", 0.04); Add("Asparrena", 0.04);
            Add("Ayala/Aiara", 0.04); Add("Barrundia", 0.04); Add("Berantevilla", 0.04); Add("Bernedo", 0.04);
            Add("Campezo/Kanpezu", 0.04); Add("Elburgo/Burgelu", 0.04); Add("Elciego", 0.04); Add("Elvillar/Bilar", 0.04);
            Add("Erriberagoitia", 0.04); Add("Harana/Valle de Arana", 0.04); Add("Iruña Oka", 0.04); Add("Iruraiz-Gauna", 0.04);
            Add("Kripan", 0.04); Add("Kuartango", 0.04); Add("Lagrán", 0.04); Add("Laguardia", 0.04); Add("Lanciego/Lantziego", 0.04);
            Add("Lantarón", 0.04); Add("Lapuebla de Labarca", 0.04); Add("Laudio/Llodio", 0.04); Add("Legutiano", 0.04);
            Add("Leza", 0.04); Add("Moreda de Álava", 0.04); Add("Navaridas", 0.04); Add("Okondo", 0.04); Add("Oyón-Oion", 0.04);
            Add("Peñacerrada-Urizaharra", 0.04); Add("Ribera Baja/Erribera Beitia", 0.04); Add("Salvatierra/Agurain", 0.04);
            Add("Samaniego", 0.04); Add("San Millán/Donemiliaga", 0.04); Add("Urkabustaiz", 0.04); Add("Valdegovía", 0.04);
            Add("Villabuena de Álava", 0.04); Add("Vitoria-Gasteiz", 0.04); Add("Yécora/Iekora", 0.04); Add("Zalduondo", 0.04);
            Add("Zambrana", 0.04); Add("Zigoitia", 0.04); Add("Zuia", 0.04);

            // GUIPÚZCOA
            Add("Abaltzisketa", 0.04); Add("Aduna", 0.04); Add("Aia", 0.04); Add("Aizarnazabal", 0.04); Add("Albiztur", 0.04);
            Add("Alegia", 0.04); Add("Alkiza", 0.04); Add("Altzaga", 0.04); Add("Altzo", 0.04); Add("Amezketa", 0.04);
            Add("Andoain", 0.04); Add("Anoeta", 0.04); Add("Antzuola", 0.04); Add("Arama", 0.04); Add("Aretxabaleta", 0.04);
            Add("Arrasate/Mondragón", 0.04); Add("Asteasu", 0.04); Add("Astigarraga", 0.04); Add("Ataun", 0.04); Add("Azkoitia", 0.04);
            Add("Azpeitia", 0.04); Add("Baliarrain", 0.04); Add("Beasain", 0.04); Add("Beizama", 0.04); Add("Belauntza", 0.04);
            Add("Berastegi", 0.04); Add("Bergara", 0.04); Add("Berrobi", 0.04); Add("Bidegoian", 0.04); Add("Deba", 0.04);
            Add("Donostia-San Sebastián", 0.04); Add("Eibar", 0.05); Add("Elduain", 0.04); Add("Elgeta", 0.04); Add("Elgoibar", 0.04);
            Add("Errenteria", 0.04); Add("Errezil", 0.04); Add("Eskoriatza", 0.04); Add("Ezkio-Itsaso", 0.04); Add("Gabiria", 0.04);
            Add("Gaintza", 0.04); Add("Gaztelu", 0.04); Add("Getaria", 0.04); Add("Hernani", 0.04); Add("Hernialde", 0.04);
            Add("Hondarribia", 0.05); Add("Ibarra", 0.04); Add("Idiazabal", 0.04); Add("Ikaztegieta", 0.04); Add("Irun", 0.05);
            Add("Irura", 0.04); Add("Itsasondo", 0.04); Add("Larraul", 0.04); Add("Lasarte-Oria", 0.04); Add("Lazkao", 0.04);
            Add("Leaburu", 0.04); Add("Legazpi", 0.04); Add("Legorreta", 0.04); Add("Leintz-Gatzaga", 0.04); Add("Lezo", 0.04);
            Add("Lizartza", 0.04); Add("Mendaro", 0.04); Add("Mutiloa", 0.04); Add("Mutriku", 0.04); Add("Oiartzun", 0.04);
            Add("Olaberria", 0.04); Add("Oñati", 0.04); Add("Ordizia", 0.04); Add("Orendain", 0.04); Add("Orexa", 0.04);
            Add("Orio", 0.04); Add("Ormaiztegi", 0.04); Add("Pasaia", 0.04); Add("Segura", 0.04);
            Add("Soraluze/Placencia de las Armas", 0.04); Add("Tolosa", 0.05); Add("Urnieta", 0.04); Add("Urretxu", 0.04);
            Add("Usurbil", 0.04); Add("Villabona", 0.04); Add("Zaldibia", 0.04); Add("Zarautz", 0.04); Add("Zegama", 0.04);
            Add("Zerain", 0.04); Add("Zestoa", 0.04); Add("Zizurkil", 0.04); Add("Zumaia", 0.04); Add("Zumarraga", 0.04);

            // VIZCAYA
            Add("Abadiño", 0.04); Add("Abanto y Ciérvana-Abanto Zierbena", 0.04); Add("Ajangiz", 0.04); Add("Alonsotegi", 0.04);
            Add("Amorebieta-Etxano", 0.04); Add("Amoroto", 0.04); Add("Arakaldo", 0.04); Add("Arantzazu", 0.04); Add("Areatza", 0.04);
            Add("Arrankudiaga", 0.04); Add("Arratzu", 0.04); Add("Arrieta", 0.04); Add("Arrigorriaga", 0.04); Add("Artea", 0.04);
            Add("Artzentales", 0.04); Add("Atxondo", 0.04); Add("Aulesti", 0.04); Add("Bakio", 0.04); Add("Balmaseda", 0.04);
            Add("Barakaldo", 0.04); Add("Barrika", 0.04); Add("Basauri", 0.04); Add("Bedia", 0.04); Add("Berango", 0.04);
            Add("Bermeo", 0.04); Add("Berriatua", 0.04); Add("Berriz", 0.04); Add("Bilbao", 0.04); Add("Busturia", 0.04);
            Add("Derio", 0.04); Add("Dima", 0.04); Add("Durango", 0.04); Add("Ea", 0.04); Add("Elantxobe", 0.04); Add("Elorrio", 0.04);
            Add("Erandio", 0.04); Add("Ereño", 0.04); Add("Ermua", 0.04); Add("Errigoiti", 0.04); Add("Etxebarri", 0.04);
            Add("Etxebarria", 0.04); Add("Forua", 0.04); Add("Fruiz", 0.04); Add("Galdakao", 0.04); Add("Galdames", 0.04);
            Add("Gamiz-Fika", 0.04); Add("Garai", 0.04); Add("Gatika", 0.04); Add("Gautegiz Arteaga", 0.04); Add("Gernika-Lumo", 0.04);
            Add("Getxo", 0.04); Add("Gizaburuaga", 0.04); Add("Gordexola", 0.04); Add("Gorliz", 0.04); Add("Güeñes", 0.04);
            Add("Ibarrangelu", 0.04); Add("Igorre", 0.04); Add("Ispaster", 0.04); Add("Iurreta", 0.04); Add("Izurtza", 0.04);
            Add("Karrantza Harana/Valle de Carranza", 0.04); Add("Kortezubi", 0.04); Add("Lanestosa", 0.04); Add("Larrabetzu", 0.04);
            Add("Laukiz", 0.04); Add("Leioa", 0.04); Add("Lekeitio", 0.04); Add("Lemoa", 0.04); Add("Lemoiz", 0.04); Add("Lezama", 0.04);
            Add("Loiu", 0.04); Add("Mallabia", 0.04); Add("Mañaria", 0.04); Add("Markina-Xemein", 0.04); Add("Maruri-Jatabe", 0.04);
            Add("Mendata", 0.04); Add("Mendexa", 0.04); Add("Meñaka", 0.04); Add("Morga", 0.04); Add("Mundaka", 0.04);
            Add("Mungia", 0.04); Add("Munitibar-Arbatzegi Gerrikaitz", 0.04); Add("Murueta", 0.04); Add("Muskiz", 0.04);
            Add("Muxika", 0.04); Add("Nabarniz", 0.04); Add("Ondarroa", 0.04); Add("Orozko", 0.04); Add("Ortuella", 0.04);
            Add("Otxandio", 0.04); Add("Plentzia", 0.04); Add("Portugalete", 0.04); Add("Santurtzi", 0.04); Add("Sestao", 0.04);
            Add("Sondika", 0.04); Add("Sopelana", 0.04); Add("Sopuerta", 0.04); Add("Sukarrieta", 0.04); Add("Trucios-Turtzioz", 0.04);
            Add("Ubide", 0.04); Add("Ugao-Miraballes", 0.04); Add("Urduliz", 0.04); Add("Urduña-Orduña", 0.04);
            Add("Valle de Trápaga-Trapagaran", 0.04); Add("Zaldibar", 0.04); Add("Zalla", 0.04); Add("Zamudio", 0.04);
            Add("Zaratamo", 0.04); Add("Zeanuri", 0.04); Add("Zeberio", 0.04); Add("Zierbena", 0.04);

            // =================================================================
            // LA RIOJA
            // =================================================================
            Add("Agoncillo", 0.04); Add("Aguilar del Río Alhama", 0.04); Add("Ajamil", 0.04); Add("Albelda de Iregua", 0.04);
            Add("Alberite", 0.04); Add("Alcanadre", 0.04); Add("Aldeanueva de Ebro", 0.04); Add("Alfaro", 0.04);
            Add("Almarza de Cameros", 0.04); Add("Arnedillo", 0.04); Add("Arnedo", 0.04); Add("Arrúbal", 0.04); Add("Ausejo", 0.04);
            Add("Autol", 0.04); Add("Bergasa", 0.04); Add("Bergasillas Bajera", 0.04); Add("Cabezón de Cameros", 0.04);
            Add("Calahorra", 0.04); Add("Cervera del Río Alhama", 0.04); Add("Clavijo", 0.04); Add("Corera", 0.04); Add("Cornago", 0.04);
            Add("Enciso", 0.04); Add("Entrena", 0.04); Add("Fuenmayor", 0.04); Add("Galilea", 0.04); Add("Gallinero de Cameros", 0.04);
            Add("Grávalos", 0.04); Add("Haro", 0.04); Add("Herce", 0.04); Add("Hornillos de Cameros", 0.04); Add("Igea", 0.04);
            Add("Jalón de Cameros", 0.04); Add("Laguna de Cameros", 0.04); Add("Lagunilla del Jubera", 0.04); Add("Lardero", 0.04);
            Add("Leza de Río Leza", 0.04); Add("Logroño", 0.04); Add("Lumbreras", 0.04); Add("Medrano", 0.04); Add("Munilla", 0.04);
            Add("Murillo de Río Leza", 0.04); Add("Muro de Aguas", 0.04); Add("Muro en Cameros", 0.04); Add("Nalda", 0.04);
            Add("Navajún", 0.04); Add("Navarrete", 0.04); Add("Nestares", 0.04); Add("Ortigosa de Cameros", 0.04); Add("Pinillos", 0.04);
            Add("Pradejón", 0.04); Add("Pradillo", 0.04); Add("Préjano", 0.04); Add("Quel", 0.04); Add("Rabanera", 0.04);
            Add("Rasillo de Cameros, El", 0.04); Add("Redal, El", 0.04); Add("Ribafrecha", 0.04); Add("Rincón de Soto", 0.04);
            Add("Robres del Castillo", 0.04); Add("San Román de Cameros", 0.04); Add("Santa Engracia del Jubera", 0.04);
            Add("Santa Eulalia Bajera", 0.04); Add("Soto en Cameros", 0.04); Add("Sojuela", 0.04); Add("Sorzano", 0.04);
            Add("Terroba", 0.04); Add("Torre en Cameros", 0.04); Add("Tudelilla", 0.04); Add("Valdemadera", 0.04);
            Add("Villar de Torre", 0.04); Add("Villarta-Quintana", 0.04); Add("Villoslada de Cameros", 0.04); Add("Villarroya", 0.04);
            Add("Zarzosa", 0.04);

            // =================================================================
            // CIUDADES AUTÓNOMAS
            // =================================================================
            Add("Ceuta", 0.05, 1.2); Add("Melilla", 0.08, 1.0);
        }

        // Método auxiliar privado para llenar el diccionario
        private static void Add(string city, double ab, double k = 1.0)
        {
            string key = city.ToLower().Trim();
            if (!_cities.ContainsKey(key))
            {
                _cities.Add(key, (ab, k));
            }
        }

        /// <summary>
        /// Intenta obtener los datos sísmicos de una ciudad.
        /// </summary>
        /// <param name="cityName">Nombre de la ciudad (insensible a mayúsculas).</param>
        /// <param name="ab">Aceleración básica de salida.</param>
        /// <param name="k">Coeficiente de contribución de salida.</param>
        /// <returns>True si encuentra la ciudad, False si no.</returns>
        public static bool TryGetCityData(string cityName, out double ab, out double k)
        {
            ab = 0.04; // Valor por defecto seguro
            k = 1.0;

            if (string.IsNullOrWhiteSpace(cityName)) return false;

            string key = cityName.ToLower().Trim();
            if (_cities.ContainsKey(key))
            {
                var data = _cities[key];
                ab = data.ab;
                k = data.K;
                return true;
            }
            return false;
        }
    }
}