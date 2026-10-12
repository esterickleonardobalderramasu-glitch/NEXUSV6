using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NexusVideojuegoEngine
{
    // =========================================================================
    // SITEMA DE JERARQUÍA DE PERSONAJES Y ENTIDADES (POO)
    // =========================================================================
    public abstract class Personaje
    {
        public string Nombre { get; protected set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int Vida { get; protected set; }
        public int VidaMax { get; protected set; }
        public bool EstaVivo => Vida > 0;

        public Personaje(string nombre, int posX, int posY, int vida)
        {
            Nombre = nombre;
            PosX = posX;
            PosY = posY;
            Vida = Math.Max(1, vida);
            VidaMax = Math.Max(1, vida);
        }

        public abstract void ReaccionarAlAnalisis();

        public virtual void RecibirDano(int cantidad)
        {
            Vida = Math.Max(0, Vida - cantidad);
        }

        public void Curar(int cantidad)
        {
            Vida = Math.Min(VidaMax, Vida + cantidad);
        }
    }

    // 1. CADETE
    public class Cadete : Personaje
    {
        public int XP { get; private set; }
        public int Rango { get; private set; }
        public List<ObjetoInventario> Inventario { get; private set; }

        public Cadete(string nombre, int posX, int posY, int vida) : base(nombre, posX, posY, vida)
        {
            XP = 0;
            Rango = 1;
            Inventario = new List<ObjetoInventario>();
        }

        public void GanarXP(int cantidad)
        {
            XP += cantidad;
            if (XP >= Rango * 100)
            {
                Rango++;
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"\n[★] ¡ASCENSO DE RANGO! Rango de Cadete alcanzado: Nivel {Rango}");
                Console.ResetColor();
            }
        }

        public override void ReaccionarAlAnalisis()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n[ANALIZADOR]: Registro Biométrico de {Nombre}. Cadete Explorer NEXUS - Año 2297.");
            Console.WriteLine($" Status: Vitalidad [{Vida}/{VidaMax}] HP | Rango: {Rango} | Experiencia: {XP} XP.");
            Console.ResetColor();
        }
    }

    // 2. ROBOT
    public class Robot : Personaje
    {
        public bool EscudoDefensivoActivo { get; private set; }
        public int Blindaje { get; private set; }

        public Robot(string nombre, int posX, int posY, int vida, int blindaje) : base(nombre, posX, posY, vida)
        {
            Blindaje = blindaje;
            EscudoDefensivoActivo = false;
        }

        public void ActivarProtocoloDefensivo()
        {
            EscudoDefensivoActivo = true;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[🤖 {Nombre}]: ¡Protocolo defensivo de emergencia activado! Blindaje suplementario al 100%.");
            Console.ResetColor();
        }

        public override void RecibirDano(int cantidad)
        {
            if (EscudoDefensivoActivo)
            {
                cantidad /= 2; // Reduce el daño a la mitad si está defendiendo
            }
            base.RecibirDano(cantidad);
        }

        public override void ReaccionarAlAnalisis()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[ANALIZADOR]: Unidad Autómata Centinela [{Nombre}].");
            Console.WriteLine($" Blindaje Estructural: {Blindaje}% | Defensas: {(EscudoDefensivoActivo ? "ACTIVADAS" : "INACTIVAS")}.");
            Console.ResetColor();
        }
    }

    // 3. ENEMIGO
    public class Enemigo : Personaje
    {
        private int danoAtaque;
        public int DanoAtaque => danoAtaque;

        public Enemigo(string nombre, int posXInicial, int posYInicial, int dano, int vidaInicial)
            : base(nombre, posXInicial, posYInicial, vidaInicial)
        {
            this.danoAtaque = dano;
        }

        public bool MoverHacia(int targetX, int targetY, int[,] mapa)
        {
            if (!EstaVivo) return false;

            int dx = Math.Sign(targetX - PosX);
            int dy = Math.Sign(targetY - PosY);

            if (dx != 0 && EsCasillaTransitable(PosX + dx, PosY, mapa))
            {
                PosX += dx;
                return true;
            }
            else if (dy != 0 && EsCasillaTransitable(PosX, PosY + dy, mapa))
            {
                PosY += dy;
                return true;
            }

            return false;
        }

        private bool EsCasillaTransitable(int x, int y, int[,] mapa)
        {
            if (x < 0 || x >= mapa.GetLength(1) || y < 0 || y >= mapa.GetLength(0))
                return false;

            int celda = mapa[y, x];
            return celda == 0 || celda == 2 || celda == 3 || celda == 4;
        }

        public override void ReaccionarAlAnalisis()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[ANALIZADOR]: Entidad Anómala Hostil [{Nombre}].");
            Console.WriteLine($" Potencial Ofensivo: {danoAtaque} de Daño | Puntos Vitales: {Vida}/{VidaMax} HP. Nivel de Amenaza Crítico.");
            Console.ResetColor();
        }
    }

    // =========================================================================
    // SISTEMA DE INVENTARIO Y OBJETOS
    // =========================================================================
    public enum TipoObjeto { Energia, Escudo, Ataque, Analizador }

    public class ObjetoInventario
    {
        public string Nombre { get; set; }
        public TipoObjeto Tipo { get; set; }
        public int Valor { get; set; }
        public string Icono { get; set; }

        public ObjetoInventario(string nombre, TipoObjeto tipo, int valor, string icono)
        {
            Nombre = nombre;
            Tipo = tipo;
            Valor = valor;
            Icono = icono;
        }
    }

    // =========================================================================
    // ENTIDAD POO: NAVE
    // =========================================================================
    public class Nave
    {
        private string nombre;
        private int combustible;
        private int saludEscudo;
        private int capacidadCarga;
        private int cargaActual;
        private bool motorHiperimpulso;

        public string Nombre => nombre;
        public int Combustible => combustible;
        public int SaludEscudo => saludEscudo;
        public int CapacidadCarga => capacidadCarga;
        public int CargaActual => cargaActual;
        public bool MotorHiperimpulso => motorHiperimpulso;
        public bool EstaOperativa => combustible > 0 && saludEscudo > 0;

        public Nave(string nombre, int combustibleInicial, int escudoInicial, int capacidadCarga, bool tieneHiperimpulso)
        {
            this.nombre = nombre;
            this.combustible = Math.Clamp(combustibleInicial, 0, 100);
            this.saludEscudo = Math.Clamp(escudoInicial, 0, 100);
            this.capacidadCarga = Math.Max(10, capacidadCarga);
            this.cargaActual = 0;
            this.motorHiperimpulso = tieneHiperimpulso;
        }

        public bool Desplazarse(int costoCombustible)
        {
            if (!EstaOperativa) return false;

            if (combustible >= costoCombustible)
            {
                combustible -= costoCombustible;
                return true;
            }
            return false;
        }

        public void RecibirDano(int cantidad)
        {
            saludEscudo = Math.Max(0, saludEscudo - cantidad);
        }

        public bool AlmacenarRecurso(int cantidad)
        {
            if (cargaActual + cantidad <= capacidadCarga)
            {
                cargaActual += cantidad;
                return true;
            }
            return false;
        }

        public void RecargarCombustible(int cantidad)
        {
            combustible = Math.Min(100, combustible + cantidad);
        }

        public void RepararEscudos(int cantidad)
        {
            saludEscudo = Math.Min(100, saludEscudo + cantidad);
        }
    }

    // =========================================================================
    // NÚCLEO MONOLÍTICO INTEGRAL DE NEXUS OS (AÑO 2297)
    // =========================================================================
    class Program
    {
        // CADETE Y ESTADO GLOBAL
        static Cadete jugadorCadete = new Cadete("NEXUS", 1, 1, 100);
        static int energiaCore = 35;
        static int mineralesRecolectados = 0;
        static int nivelAmenaza = 1;
        static bool irisConectado = false;
        static bool tieneLlave = false;

        // OBJETIVOS DE LA MISIÓN "LA ANOMALÍA PERDIDA"
        static bool paso1ZonaAlcanzada = false;
        static bool paso2NucleoEncontrado = false;
        static bool paso3EntidadSobrevivida = false;
        static bool paso4RegresoExtraccion = false;
        static bool misionAnomaliaCompletada = false;

        // DIRECTIVAS Y MISIONES HISTÓRICAS
        static bool misionRecoleccionCompleta = false;
        static bool misionInfiltracionCompleta = false;
        static bool misionRedCompleta = false;

        // COMUNICACIÓN TCP
        static int puertoTCP = 8080;
        static string ipRemota = "127.0.0.1";
        static bool redConectada = false;
        static TcpClient? socketClienteRed = null;
        static NetworkStream? streamRed = null;

        // HANGAR DE NAVES (POO)
        static List<Nave> hangarNaves = new List<Nave>()
        {
            new Nave("NEXUS-Vanguard (Exploración 2297)", 100, 80, 50, true),
            new Nave("Goliath-Cargo (Carguero Táctico)", 60, 100, 250, false)
        };
        static int naveSeleccionadaIndex = 0;

        // BITÁCORA DEL SISTEMA
        static List<string> bitacoraRegistros = new List<string>();

        // REALIDADES Y MATRICES DE MAPA (0:Vacío, 1:Muro, 2:Mineral, 3:Peligro, 4:Llave, 5:Puerta)
        static int[,] mapaAlfa = new int[8, 16]
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 0, 0, 0, 1, 2, 0, 0, 0, 0, 0, 0, 0, 2, 0, 1 },
            { 1, 0, 1, 0, 1, 0, 1, 1, 1, 4, 1, 1, 1, 0, 0, 1 },
            { 1, 0, 1, 0, 0, 0, 0, 2, 1, 0, 0, 5, 1, 0, 0, 1 },
            { 1, 2, 1, 1, 1, 0, 1, 0, 1, 0, 3, 0, 1, 0, 0, 1 },
            { 1, 0, 0, 3, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1 },
            { 1, 0, 2, 0, 0, 0, 1, 2, 0, 0, 0, 2, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        };

        static int[,] mapaBeta = new int[8, 16]
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 0, 3, 0, 0, 0, 1, 2, 0, 0, 3, 0, 0, 2, 0, 1 },
            { 1, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 1, 1, 0, 0, 1 },
            { 1, 2, 1, 5, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 1 },
            { 1, 0, 1, 0, 1, 1, 1, 4, 1, 1, 1, 0, 1, 3, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 1, 0, 0, 2, 1, 0, 0, 0, 0, 1 },
            { 1, 3, 2, 0, 3, 0, 0, 0, 3, 0, 0, 0, 2, 0, 0, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        };

        static int[,] mapaGamma = new int[8, 16]
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 2, 0, 0, 3, 0, 2, 0, 0, 3, 0, 0, 2, 0, 2, 1 },
            { 1, 0, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 0, 1 },
            { 1, 0, 1, 4, 0, 0, 0, 2, 0, 0, 0, 1, 0, 1, 0, 1 },
            { 1, 3, 1, 1, 1, 1, 0, 1, 1, 1, 0, 5, 0, 1, 3, 1 },
            { 1, 0, 0, 0, 2, 0, 0, 0, 2, 0, 0, 1, 0, 0, 0, 1 },
            { 1, 2, 3, 0, 0, 3, 2, 0, 0, 3, 0, 0, 2, 3, 2, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        };

        static int[,] mapaDelta = new int[8, 16]
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 1 },
            { 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1 },
            { 1, 0, 1, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 0, 0, 1 },
            { 1, 0, 1, 2, 4, 0, 0, 0, 0, 5, 2, 1, 1, 0, 0, 1 },
            { 1, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1 },
            { 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        };

        static int[,] mapaActivo = mapaAlfa;
        static string nombreSectorActivo = "Realidad Alfa (Superficie Anómala)";
        static int playerX = 1;
        static int playerY = 1;

        // ENEMIGOS Y PERSONAJES POR REALIDAD
        static List<Personaje> listaEntidadesAlfa = new List<Personaje>()
        {
            new Enemigo("Patrullador Alfa 1", 9, 1, 15, 30),
            new Robot("Centinela R2-Nexus", 5, 3, 50, 80),
            new Enemigo("Patrullador Alfa 2", 9, 5, 20, 45)
        };

        static List<Personaje> listaEntidadesBeta = new List<Personaje>()
        {
            new Enemigo("Espectro Beta 1", 9, 1, 20, 40),
            new Robot("Unidad Minera B-3", 3, 5, 60, 90),
            new Enemigo("Espectro Beta 2", 12, 5, 25, 50)
        };

        static List<Personaje> listaEntidadesGamma = new List<Personaje>()
        {
            new Enemigo("Anomalía Cuántica 1", 5, 2, 30, 60),
            new Enemigo("Entidad Cósmica Central", 8, 3, 40, 100),
            new Robot("Defensor Cuántico R-9", 10, 4, 70, 95)
        };

        static List<Personaje> listaEntidadesDelta = new List<Personaje>()
        {
            new Enemigo("Centinela Estación 1", 13, 1, 40, 80),
            new Robot("Androide Guardián D-0", 7, 4, 80, 100)
        };

        static List<Personaje> listaEntidadesActivas = listaEntidadesAlfa;

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "NEXUS OS v6.5 — Año 2297: Misión Anomalía Perdida";

            SecuenciaBooteoConPersonalizacion();
            InicializarInventarioInicial();

            string[] opcionesMenu = {
                " PERFIL DEL CADETE EXPLORADOR Y CORE",
                " DIAGNÓSTICO DEL NÚCLEO Y BITÁCORA HISTÓRICA",
                " GESTIÓN DE INVENTARIO Y EQUIPAMIENTO (AÑO 2297)",
                " MISIONES DE EXPLORACIÓN: 'LA ANOMALÍA PERDIDA'",
                " EXPLORAR ZONAS Y ENFRENTAR EVENTOS DINÁMICOS",
                " SISTEMA DE ENCUENTROS DIRECTOS Y COMBATE TÁCTICO",
                " GESTIÓN DE HANGAR Y NAVES (POO)",
                " TERMINAL INTERACTIVA DE ENTIDAD IRIS",
                " CONEXIÓN Y TRANSICIÓN DE RED POR PUERTO (TCP)",
                " DESPLIEGUE A MATRIZ DE SIMULACIÓN 2D EN TIEMPO REAL",
                " VIAJAR A OTRA REALIDAD / SECTOR DIMENSIONAL",
                " SALIR DEL SISTEMA"
            };

            int seleccion = 0;
            bool ejecutando = true;

            do
            {
                Console.CursorVisible = false;
                Console.Clear();
                DibujarBannerHeader();

                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("╔═══════════════════════════════════════════════════════════════════════╗");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("║        NEXUS ENGINE v6.5 — MISIONES DE CADETE DE EXPLORACIÓN 2297     ║");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine("╠═══════════════════════════════════════════════════════════════════════╣");

                for (int i = 0; i < opcionesMenu.Length; i++)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write("║");

                    if (i == seleccion)
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write($"  ► {opcionesMenu[i].PadRight(65)} ");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write($"    {opcionesMenu[i].PadRight(65)} ");
                        Console.ResetColor();
                    }

                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine("║");
                }

                Console.WriteLine("╚═══════════════════════════════════════════════════════════════════════╝");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\n Controles: [↑ / ↓ / W / S] Navegar  │  [ENTER] Seleccionar");
                Console.ResetColor();

                ConsoleKeyInfo tecla = Console.ReadKey(true);

                if (tecla.Key == ConsoleKey.UpArrow || tecla.Key == ConsoleKey.W)
                    seleccion = (seleccion == 0) ? opcionesMenu.Length - 1 : seleccion - 1;
                else if (tecla.Key == ConsoleKey.DownArrow || tecla.Key == ConsoleKey.S)
                    seleccion = (seleccion == opcionesMenu.Length - 1) ? 0 : seleccion + 1;
                else if (tecla.Key == ConsoleKey.Enter)
                {
                    Console.CursorVisible = true;
                    switch (seleccion)
                    {
                        case 0: ModuloConfigurarPerfilYEnergia(); break;
                        case 1: ModuloDiagnostico(); break;
                        case 2: ModuloGestionInventario(); break;
                        case 3: ModuloMisionAnomaliaPerdida(); break;
                        case 4: ModuloExploracionZonasEventos(); break;
                        case 5: ModuloEncuentrosDirectos(); break;
                        case 6: ModuloHangarNaves(); break;
                        case 7: ModuloEnlaceIris(); break;
                        case 8: ModuloConexionPuertoTCP(); break;
                        case 9: ModuloJuegoModo2D(); break;
                        case 10: ModuloSeleccionarMapa(); break;
                        case 11:
                            ejecutando = false;
                            CerrarConexionRed();
                            break;
                    }
                }

            } while (ejecutando);
        }

        static void SecuenciaBooteoConPersonalizacion()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
   _  _______ _  _    ____ 
  / |/ / __/ |/ / / / / __/
 /    / _//    / /_/ /\ \  
/_/|_/___/_/|_/\____/___/   v6.5 (Año 2297 - Edición Cadete)
            ");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("=========================================================================");
            Console.WriteLine(" AÑO 2297. El Cadete Explorer NEXUS recibe una nueva misión:");
            Console.WriteLine(" Investigar una anomalía detectada en una realidad desconocida.");
            Console.WriteLine(" Deberás explorar, recolectar recursos, administrar tu inventario,");
            Console.WriteLine(" enfrentarte a entidades singulares y regresar con vida para triunfar.");
            Console.WriteLine("=========================================================================\n");
            Console.ResetColor();

            Thread.Sleep(800);

            Console.Write("► Ingrese la Identificación del Cadete Explorer [Predeterminado: Cadete NEXUS]: ");
            string? inputNombre = Console.ReadLine()?.Trim();
            string nombreFinal = string.IsNullOrEmpty(inputNombre) ? "Cadete NEXUS" : inputNombre;

            jugadorCadete = new Cadete(nombreFinal, 1, 1, 100);

            Console.Write("► Nivel de Energía inicial del Core (10 - 100) [Predeterminado: 35]: ");
            string? inputEnergiaStr = Console.ReadLine()?.Trim();
            if (int.TryParse(inputEnergiaStr, out int inputEnergia))
            {
                energiaCore = Math.Clamp(inputEnergia, 10, 100);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n[✓] Cadete '{jugadorCadete.Nombre}' autenticado con {energiaCore}% de Energía y {jugadorCadete.Vida} HP de Core.");
            Console.ResetColor();
            RegistrarEnBitacora($"Booteo 2297 completado. Cadete: {jugadorCadete.Nombre} | Energía: {energiaCore}% | HP: {jugadorCadete.Vida}%.");
            Thread.Sleep(1000);
        }

        static void InicializarInventarioInicial()
        {
            jugadorCadete.Inventario.Add(new ObjetoInventario("Célula de energía 🔋", TipoObjeto.Energia, 25, "🔋"));
            jugadorCadete.Inventario.Add(new ObjetoInventario("Escudo de Plasma 🛡️", TipoObjeto.Escudo, 30, "🛡️"));
            jugadorCadete.Inventario.Add(new ObjetoInventario("Módulo de ataque Léser 🔫", TipoObjeto.Ataque, 20, "🔫"));
            jugadorCadete.Inventario.Add(new ObjetoInventario("Analizador Cuántico 🔬", TipoObjeto.Analizador, 10, "🔬"));
        }

        static void ModuloConfigurarPerfilYEnergia()
        {
            Console.Clear();
            DibujarBannerHeader();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== PERFIL DEL CADETE EXPLORADOR Y CORE ===\n");
            Console.ResetColor();

            Console.WriteLine($"Cadete Registrado:   {jugadorCadete.Nombre}");
            Console.WriteLine($"Rango Actual:        Nivel {jugadorCadete.Rango}");
            Console.WriteLine($"Experiencia (XP):   {jugadorCadete.XP} XP");
            Console.WriteLine($"Energía del Core:    {energiaCore}%");
            Console.WriteLine($"Salud HP del Core:   {jugadorCadete.Vida}/{jugadorCadete.VidaMax} HP\n");

            Console.WriteLine(" [1] Cambiar Identificador del Cadete");
            Console.WriteLine(" [2] Ajustar Energía del Core");
            Console.WriteLine(" [3] Reparar Salud HP del Core");
            Console.WriteLine(" [4] Volver al menú principal");
            Console.Write("\nSelección > ");

            string? op = Console.ReadLine()?.Trim();
            if (op == "1")
            {
                Console.Write("\nNuevo Nombre de Cadete: ");
                string? n = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(n))
                {
                    jugadorCadete = new Cadete(n, jugadorCadete.PosX, jugadorCadete.PosY, jugadorCadete.Vida);
                    RegistrarEnBitacora($"Cadete renombrado a {jugadorCadete.Nombre}.");
                }
            }
            else if (op == "2")
            {
                Console.Write("\nNuevo Nivel de Energía (10-100): ");
                if (int.TryParse(Console.ReadLine(), out int e))
                {
                    energiaCore = Math.Clamp(e, 10, 100);
                    RegistrarEnBitacora($"Energía ajustada a {energiaCore}%.");
                }
                else
                {
                    MostrarError("Entrada inválida. Debe ingresar un valor numérico.");
                }
            }
            else if (op == "3")
            {
                Console.Write("\nRestaurar Salud HP (10-100): ");
                if (int.TryParse(Console.ReadLine(), out int h))
                {
                    jugadorCadete.Curar(h);
                    RegistrarEnBitacora($"Salud de Cadete ajustada a {jugadorCadete.Vida} HP.");
                }
                else
                {
                    MostrarError("Entrada inválida. Debe ingresar un valor numérico.");
                }
            }
            Pausar();
        }

        static void ModuloDiagnostico()
        {
            Console.Clear();
            DibujarBannerHeader();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== DIAGNÓSTICO DEL NÚCLEO Y BITÁCORA HISTÓRICA ===\n");
            Console.ResetColor();

            Console.WriteLine($"Cadete Explorador:         {jugadorCadete.Nombre} (Rango {jugadorCadete.Rango})");
            Console.WriteLine($"Salud del Core (HP):      {jugadorCadete.Vida}/{jugadorCadete.VidaMax}");
            Console.WriteLine($"Energía Reserva:          {energiaCore}%");
            Console.WriteLine($"Minerales en Reserva:     {mineralesRecolectados}");
            Console.WriteLine($"Nivel de Amenaza Sector:  Nivel {nivelAmenaza}");
            Console.WriteLine($"Conexión IRIS:            {(irisConectado ? "ESTABLECIDA" : "PENDIENTE")}");
            Console.WriteLine($"Enlace TCP:               {(redConectada ? "ACTIVO" : "DESCONECTADO")}\n");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("--- HISTORIAL DE BITÁCORA HISTÓRICA ---");
            Console.ResetColor();

            if (bitacoraRegistros.Count == 0)
            {
                Console.WriteLine("(No hay eventos registrados)");
            }
            else
            {
                foreach (string reg in bitacoraRegistros)
                {
                    Console.WriteLine($" • {reg}");
                }
            }
            Pausar();
        }

        // =========================================================================
        // MÓDULO DE INVENTARIO INTERACTIVO (AGREGAR -> CONSULTAR -> USAR -> ELIMINAR)
        // =========================================================================
        static void ModuloGestionInventario()
        {
            bool enInventario = true;
            while (enInventario)
            {
                Console.Clear();
                DibujarBannerHeader();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n=== GESTIÓN DE INVENTARIO Y EQUIPAMIENTO (CADETE 2297) ===\n");
                Console.ResetColor();

                Console.WriteLine("Objetos en Mochila Táctica:\n");
                var inv = jugadorCadete.Inventario;

                if (inv.Count == 0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("  (El inventario está completamente vacío)");
                    Console.ResetColor();
                }
                else
                {
                    for (int i = 0; i < inv.Count; i++)
                    {
                        Console.WriteLine($" [{i + 1}] {inv[i].Icono} {inv[i].Nombre} | Tipo: {inv[i].Tipo} | Efecto/Valor: +{inv[i].Valor}");
                    }
                }

                Console.WriteLine("\nAcciones de Inventario:");
                Console.WriteLine(" [1] Utilizar objeto");
                Console.WriteLine(" [2] Agregar objeto manualmente al inventario");
                Console.WriteLine(" [3] Eliminar/Descartar un objeto");
                Console.WriteLine(" [4] Volver al menú principal");
                Console.Write("\nSelección > ");

                string? op = Console.ReadLine()?.Trim();
                if (op == "1")
                {
                    if (inv.Count == 0)
                    {
                        MostrarError("No posees objetos para utilizar en este momento.");
                    }
                    else
                    {
                        Console.Write($"Ingrese número de objeto a utilizar (1-{inv.Count}): ");
                        if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= inv.Count)
                        {
                            ObjetoInventario obj = inv[idx - 1];
                            AplicarEfectoObjeto(obj);
                            inv.RemoveAt(idx - 1);
                        }
                        else
                        {
                            MostrarError("Selección de objeto inválida o inexistente.");
                        }
                    }
                    Pausar();
                }
                else if (op == "2")
                {
                    Console.WriteLine("\nSeleccione tipo de objeto a agregar:");
                    Console.WriteLine(" [1] 🔋 Célula de energía (+25 Energía)");
                    Console.WriteLine(" [2] 🛡️ Escudo de Plasma (+30 HP Core)");
                    Console.WriteLine(" [3] 🔫 Módulo de ataque Láser (+20 Potencia)");
                    Console.WriteLine(" [4] 🔬 Analizador Cuántico");
                    Console.Write("Selección > ");
                    string? subOp = Console.ReadLine()?.Trim();

                    if (subOp == "1")
                        inv.Add(new ObjetoInventario("Célula de energía 🔋", TipoObjeto.Energia, 25, "🔋"));
                    else if (subOp == "2")
                        inv.Add(new ObjetoInventario("Escudo de Plasma 🛡️", TipoObjeto.Escudo, 30, "🛡️"));
                    else if (subOp == "3")
                        inv.Add(new ObjetoInventario("Módulo de ataque Láser 🔫", TipoObjeto.Ataque, 20, "🔫"));
                    else if (subOp == "4")
                        inv.Add(new ObjetoInventario("Analizador Cuántico 🔬", TipoObjeto.Analizador, 10, "🔬"));
                    else
                        MostrarError("Opción de objeto desconocida.");

                    RegistrarEnBitacora("Nuevo objeto guardado en el inventario del Cadete.");
                    Pausar();
                }
                else if (op == "3")
                {
                    if (inv.Count == 0)
                    {
                        MostrarError("No hay objetos que eliminar.");
                    }
                    else
                    {
                        Console.Write($"Ingrese número de objeto a descartar (1-{inv.Count}): ");
                        if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= inv.Count)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"[!] Objeto '{inv[idx - 1].Nombre}' descartado.");
                            Console.ResetColor();
                            inv.RemoveAt(idx - 1);
                            RegistrarEnBitacora("Objeto eliminado del inventario.");
                        }
                        else
                        {
                            MostrarError("Número de objeto inválido.");
                        }
                    }
                    Pausar();
                }
                else if (op == "4")
                {
                    enInventario = false;
                }
                else
                {
                    MostrarError("Opción no válida.");
                    Pausar();
                }
            }
        }

        static void AplicarEfectoObjeto(ObjetoInventario obj)
        {
            if (obj.Tipo == TipoObjeto.Energia)
            {
                int prev = energiaCore;
                energiaCore = Math.Min(100, energiaCore + obj.Valor);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[✓] {obj.Nombre} activada. Energía: {prev}% ──► {energiaCore}%");
                Console.ResetColor();
                RegistrarEnBitacora($"Célula consumida. Energía: {prev}% -> {energiaCore}%.");
            }
            else if (obj.Tipo == TipoObjeto.Escudo)
            {
                int prevHp = jugadorCadete.Vida;
                jugadorCadete.Curar(obj.Valor);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[✓] {obj.Nombre} desplegado. HP Core: {prevHp} ──► {jugadorCadete.Vida}");
                Console.ResetColor();
                RegistrarEnBitacora($"Escudo aplicado. HP: {prevHp} -> {jugadorCadete.Vida}.");
            }
            else if (obj.Tipo == TipoObjeto.Ataque)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"\n[✓] {obj.Nombre} equipado. Sobrecarga táctica +{obj.Valor} de daño en próximo combate.");
                Console.ResetColor();
                RegistrarEnBitacora("Módulo de ataque equipado.");
            }
            else if (obj.Tipo == TipoObjeto.Analizador)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n[✓] {obj.Nombre} ejecutado. Escaneo de entidades cercanas y patrones anómalos listo.");
                Console.ResetColor();
                RegistrarEnBitacora("Analizador ejecutado.");
            }
        }

        // =========================================================================
        // MÓDULO DE MISIÓN: "LA ANOMALÍA PERDIDA"
        // =========================================================================
        static void ModuloMisionAnomaliaPerdida()
        {
            Console.Clear();
            DibujarBannerHeader();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n=== MISIÓN TÁCTICA 2297: 'LA ANOMALÍA PERDIDA' ===\n");
            Console.ResetColor();

            Console.WriteLine("Directivas del Comando Explorer para el Cadete:\n");
            Console.WriteLine($" [ {(paso1ZonaAlcanzada ? "✓" : " ")} ] 1. Llegar a la zona de anomalía (Realidad Gamma o explorar sector)");
            Console.WriteLine($" [ {(paso2NucleoEncontrado ? "✓" : " ")} ] 2. Encontrar el núcleo energético anómalo");
            Console.WriteLine($" [ {(paso3EntidadSobrevivida ? "✓" : " ")} ] 3. Sobrevivir al encuentro con la entidad desconocida");
            Console.WriteLine($" [ {(paso4RegresoExtraccion ? "✓" : " ")} ] 4. Regresar al punto de extracción en la Nave\n");

            if (!misionAnomaliaCompletada)
            {
                Console.WriteLine("Opciones de Progreso:");
                Console.WriteLine(" [1] Investigar y avanzar en la Misión");
                Console.WriteLine(" [2] Volver al menú");
                Console.Write("\nSelección > ");

                string? op = Console.ReadLine()?.Trim();
                if (op == "1")
                {
                    EjecutarPasosMisionAnomalia();
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("🟢 MISIÓN COMPLETADA CON ÉXITO");
                Console.WriteLine(" Recompensas Otorgadas:");
                Console.WriteLine("  • +100 XP de Cadete");
                Console.WriteLine("  • +1 Módulo Desconocido de Vanguardia 🔬");
                Console.ResetColor();
                Pausar();
            }
        }

        static void EjecutarPasosMisionAnomalia()
        {
            Console.Clear();
            DibujarBannerHeader();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n--- SECUENCIA DE EXPLORACIÓN DE LA ANOMALÍA PERDIDA ---\n");
            Console.ResetColor();

            if (!paso1ZonaAlcanzada)
            {
                Console.WriteLine("[PASO 1]: Escaneando coordenadas dimensionales de la anomalía...");
                Thread.Sleep(700);
                paso1ZonaAlcanzada = true;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[✓] ¡Zona de anomalía alcanzada con éxito!");
                Console.ResetColor();
                RegistrarEnBitacora("Misión: Zona de anomalía alcanzada.");
            }
            else if (!paso2NucleoEncontrado)
            {
                Console.WriteLine("[PASO 2]: Buscando firma cuántica del Núcleo Energético...");
                Thread.Sleep(700);
                paso2NucleoEncontrado = true;
                jugadorCadete.Inventario.Add(new ObjetoInventario("Núcleo Energético Anómalo ⚡", TipoObjeto.Energia, 50, "⚡"));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[✓] ¡Núcleo Energético hallado y asegurado en el inventario!");
                Console.ResetColor();
                RegistrarEnBitacora("Misión: Núcleo energético recuperado.");
            }
            else if (!paso3EntidadSobrevivida)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("⚡ ¡ALERTA! Una entidad hostil ha emergido del centro de la anomalía!");
                Console.ResetColor();
                Thread.Sleep(800);

                Enemigo guardianAnomalo = new Enemigo("Entidad Guardián Anómalo", 0, 0, 25, 60);
                SimularEncuentroEntidad(guardianAnomalo);

                if (jugadorCadete.Vida > 0)
                {
                    paso3EntidadSobrevivida = true;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n[✓] Has sobrevivido al encuentro con la entidad anómala.");
                    Console.ResetColor();
                    RegistrarEnBitacora("Misión: Sobrevivido encuentro con entidad anómala.");
                }
            }
            else if (!paso4RegresoExtraccion)
            {
                Console.WriteLine("[PASO 4]: Iniciando secuencia de retorno a la Nave de extracción...");
                Thread.Sleep(800);
                paso4RegresoExtraccion = true;
                misionAnomaliaCompletada = true;

                jugadorCadete.GanarXP(100);
                jugadorCadete.Inventario.Add(new ObjetoInventario("Módulo Desconocido Vanguardia 🔬", TipoObjeto.Analizador, 50, "🔬"));

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n=========================================================");
                Console.WriteLine("🟢 MISIÓN COMPLETADA");
                Console.WriteLine(" +100 XP Ganados");
                Console.WriteLine(" +1 Módulo Desconocido agregado al inventario");
                Console.WriteLine(" Cadete retornado con vida a salvo.");
                Console.WriteLine("=========================================================");
                Console.ResetColor();
                RegistrarEnBitacora("🟢 Misión 'La anomalía perdida' completada exitosamente.");
            }

            Pausar();
        }

        // =========================================================================
        // MÓDULO DE EXPLORACIÓN DE ZONAS Y EVENTOS ALEATORIOS (REQUERIMIENTO 5 Y 6)
        // =========================================================================
        static void ModuloExploracionZonasEventos()
        {
            Console.Clear();
            DibujarBannerHeader();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n=== EXPLORACIÓN TÁCTICA DE ZONAS [{nombreSectorActivo}] ===\n");
            Console.ResetColor();

            Console.WriteLine("Desplazando Cadete por sectores inexplorados...");
            Thread.Sleep(800);

            Random rng = new Random();
            int evento = rng.Next(1, 6);

            switch (evento)
            {
                case 1:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚡ ANOMALÍA DETECTADA");
                    Console.WriteLine(" Un pulso electromagnético sacude el traje del Cadete.");
                    Console.ResetColor();
                    energiaCore = Math.Max(0, energiaCore - 10);
                    Console.WriteLine(" Consecuencia: Energía del Core reducida en -10%.");
                    RegistrarEnBitacora("Evento: Anomalía detectada (-10% Energía).");
                    break;

                case 2:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("🔋 PÉRDIDA DE ENERGÍA");
                    Console.WriteLine(" Las condiciones climáticas del sector drenan los condensadores.");
                    Console.ResetColor();
                    energiaCore = Math.Max(0, energiaCore - 15);
                    Console.WriteLine(" Consecuencia: -15% de Energía.");
                    RegistrarEnBitacora("Evento: Pérdida de energía por ambiente.");
                    break;

                case 3:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("👾 ENTIDAD DETECTADA");
                    Console.WriteLine(" Un biomarcador hostil ha bloqueado tu posición.");
                    Console.ResetColor();
                    Personaje ent = listaEntidadesActivas[rng.Next(listaEntidadesActivas.Count)];
                    SimularEncuentroEntidad(ent);
                    break;

                case 4:
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("🎁 OBJETO ENCONTRADO");
                    ObjetoInventario nuevoObj = ObtenerObjetoAleatorio();
                    jugadorCadete.Inventario.Add(nuevoObj);
                    Console.WriteLine($" Cadete ha encontrado: {nuevoObj.Icono} {nuevoObj.Nombre} (Guardado en inventario)");
                    RegistrarEnBitacora($"Objeto hallado en exploración: {nuevoObj.Nombre}");
                    break;

                case 5:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("🚨 SISTEMA DAÑADO");
                    Console.WriteLine(" Colisión con micro-escombros dimensionales.");
                    Console.ResetColor();
                    jugadorCadete.RecibirDano(15);
                    Console.WriteLine($" Consecuencia: HP del Core dañado (-15 HP). Vitalidad actual: {jugadorCadete.Vida} HP.");
                    RegistrarEnBitacora("Evento: Daño en sistemas por micro-escombros.");
                    break;
            }

            VerificarMisiones();
            Pausar();
        }

        static ObjetoInventario ObtenerObjetoAleatorio()
        {
            Random r = new Random();
            int val = r.Next(1, 5);
            return val switch
            {
                1 => new ObjetoInventario("Célula de energía 🔋", TipoObjeto.Energia, 25, "🔋"),
                2 => new ObjetoInventario("Escudo de Plasma 🛡️", TipoObjeto.Escudo, 30, "🛡️"),
                3 => new ObjetoInventario("Módulo de ataque Láser 🔫", TipoObjeto.Ataque, 20, "🔫"),
                _ => new ObjetoInventario("Analizador Cuántico 🔬", TipoObjeto.Analizador, 15, "🔬")
            };
        }

        // =========================================================================
        // MÓDULO DE ENCUENTROS DIRECTOS (SISTEMA DE DECISIONES Y CONSECUENCIAS)
        // =========================================================================
        static void ModuloEncuentrosDirectos()
        {
            Console.Clear();
            DibujarBannerHeader();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== SISTEMA DE ENCUENTROS DIRECTOS Y COMBATE TÁCTICO ===\n");
            Console.ResetColor();

            Console.WriteLine($"Entidades presentes en {nombreSectorActivo}:\n");
            for (int i = 0; i < listaEntidadesActivas.Count; i++)
            {
                Personaje p = listaEntidadesActivas[i];
                string tipo = p is Robot ? "[ROBOT Centinela]" : (p is Enemigo ? "[ENEMIGO Hostil]" : "[CADETE]");
                Console.WriteLine($" [{i + 1}] {tipo} {p.Nombre} - Estado: {(p.EstaVivo ? $"{p.Vida}/{p.VidaMax} HP" : "DESTRUIDO/INACTIVO")}");
            }

            Console.Write("\nSeleccione entidad para interactuar (1-" + listaEntidadesActivas.Count + "): ");
            if (int.TryParse(Console.ReadLine(), out int sel) && sel >= 1 && sel <= listaEntidadesActivas.Count)
            {
                Personaje objetivo = listaEntidadesActivas[sel - 1];
                SimularEncuentroEntidad(objetivo);
            }
            else
            {
                MostrarError("Selección inválida o entidad inexistente.");
                Pausar();
            }
        }

        static void SimularEncuentroEntidad(Personaje entidad)
        {
            if (!entidad.EstaVivo)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"\n[!] La entidad {entidad.Nombre} ya se encuentra inactiva o destruida.");
                Console.ResetColor();
                return;
            }

            bool interactuando = true;

            while (interactuando && entidad.EstaVivo && jugadorCadete.Vida > 0)
            {
                Console.Clear();
                DibujarBannerHeader();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n=== ENCUENTRO EN PROGRESO: {entidad.Nombre.ToUpper()} ===\n");
                Console.ResetColor();

                Console.WriteLine($"Cadete: {jugadorCadete.Nombre} | HP: {jugadorCadete.Vida}/{jugadorCadete.VidaMax} | Energía: {energiaCore}%");
                Console.WriteLine($"Entidad: {entidad.Nombre} | HP: {entidad.Vida}/{entidad.VidaMax}\n");

                Console.WriteLine("Opciones Tácticas:");
                Console.WriteLine(" [1] Atacar ⚔️");
                Console.WriteLine(" [2] Escapar 🏃");
                Console.WriteLine(" [3] Utilizar un objeto del Inventario 🎒");
                Console.WriteLine(" [4] Analizar la entidad 🔬");
                Console.Write("\nSelección de Acción > ");

                string? op = Console.ReadLine()?.Trim();

                if (op == "1")
                {
                    // ATACAR
                    int danoCadete = 25;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n[⚔️] Atacas a {entidad.Nombre} infligiendo {danoCadete} de daño.");
                    Console.ResetColor();
                    entidad.RecibirDano(danoCadete);

                    if (entidad.EstaVivo)
                    {
                        // CONSECUENCIA SEGÚN TIPO DE ENTIDAD
                        if (entidad is Robot bot)
                        {
                            bot.ActivarProtocoloDefensivo();
                            jugadorCadete.RecibirDano(10);
                            Console.WriteLine($"[🤖 Robot]: El robot activa su sistema defensivo y contraataca (-10 HP al Cadete).");
                        }
                        else if (entidad is Enemigo ene)
                        {
                            jugadorCadete.RecibirDano(ene.DanoAtaque);
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"[👾 Enemigo]: El enemigo inicia combate furioso infligiendo -{ene.DanoAtaque} HP al Cadete.");
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"\n[✓] ¡Has neutralizado a {entidad.Nombre}!");
                        Console.ResetColor();
                        jugadorCadete.GanarXP(35);
                        RegistrarEnBitacora($"Entidad {entidad.Nombre} neutralizada.");
                    }
                    Pausar();
                }
                else if (op == "2")
                {
                    // ESCAPAR
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("\n[🏃] Intentas retirarte tácticamente del encuentro...");
                    Console.ResetColor();
                    Random r = new Random();
                    if (r.Next(0, 2) == 0)
                    {
                        Console.WriteLine(" [✓] ¡Has escapado con éxito sin sufrir daños!");
                        interactuando = false;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(" [X] ¡Fallo al escapar! La entidad te alcanza al retirarte.");
                        Console.ResetColor();
                        jugadorCadete.RecibirDano(15);
                    }
                    Pausar();
                }
                else if (op == "3")
                {
                    // UTILIZAR OBJETO
                    if (jugadorCadete.Inventario.Count == 0)
                    {
                        MostrarError("No posees objetos en el inventario.");
                    }
                    else
                    {
                        Console.WriteLine("\nObjetos Disponibles:");
                        for (int i = 0; i < jugadorCadete.Inventario.Count; i++)
                        {
                            var o = jugadorCadete.Inventario[i];
                            Console.WriteLine($" [{i + 1}] {o.Icono} {o.Nombre}");
                        }
                        Console.Write("Seleccione objeto: ");
                        if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= jugadorCadete.Inventario.Count)
                        {
                            ObjetoInventario o = jugadorCadete.Inventario[idx - 1];
                            AplicarEfectoObjeto(o);
                            jugadorCadete.Inventario.RemoveAt(idx - 1);
                        }
                        else
                        {
                            MostrarError("Objeto no válido.");
                        }
                    }
                    Pausar();
                }
                else if (op == "4")
                {
                    // ANALIZAR
                    entidad.ReaccionarAlAnalisis();
                    Pausar();
                }
                else
                {
                    MostrarError("Opción no válida. Debes elegir una acción correcta.");
                    Pausar();
                }
            }
        }

        static void ModuloHangarNaves()
        {
            bool enHangar = true;
            while (enHangar)
            {
                Console.Clear();
                DibujarBannerHeader();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n=== MÓDULO DE HANGAR DE NAVES (OBJETOS POO) ===\n");
                Console.ResetColor();

                Nave naveActiva = hangarNaves[naveSeleccionadaIndex];

                Console.WriteLine($"Nave Asignada Actualmente: [{naveActiva.Nombre}]\n");

                for (int i = 0; i < hangarNaves.Count; i++)
                {
                    Nave n = hangarNaves[i];
                    string marca = (i == naveSeleccionadaIndex) ? "[ACTIVA]" : "        ";
                    Console.ForegroundColor = (i == naveSeleccionadaIndex) ? ConsoleColor.Green : ConsoleColor.Gray;
                    Console.WriteLine($" {marca} [{i + 1}] {n.Nombre}");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"          Combustible: {n.Combustible}% | Escudos: {n.SaludEscudo}% | Carga: {n.CargaActual}/{n.CapacidadCarga} | Hiperimpulso: {(n.MotorHiperimpulso ? "SÍ" : "NO")}");
                    Console.ResetColor();
                }

                Console.WriteLine("\nOpciones del Hangar:");
                Console.WriteLine(" [1] Cambiar nave activa de transporte");
                Console.WriteLine(" [2] Realizar prueba de vuelo táctico (-15% combustible)");
                Console.WriteLine(" [3] Cargar minerales extraídos a la nave (+20 unids)");
                Console.WriteLine(" [4] Recargar combustible (+30%)");
                Console.WriteLine(" [5] Reparar escudos de la nave (+30%)");
                Console.WriteLine(" [6] Volver al menú principal");
                Console.Write("\nSelección > ");

                string? op = Console.ReadLine()?.Trim();
                if (op == "1")
                {
                    Console.Write("Ingrese número de nave (1 o 2): ");
                    if (int.TryParse(Console.ReadLine(), out int idx) && idx >= 1 && idx <= hangarNaves.Count)
                    {
                        naveSeleccionadaIndex = idx - 1;
                        RegistrarEnBitacora($"Nave activa asignada a: {hangarNaves[naveSeleccionadaIndex].Nombre}");
                    }
                    else
                    {
                        MostrarError("Nave seleccionada no válida.");
                    }
                    Pausar();
                }
                else if (op == "2")
                {
                    if (naveActiva.Desplazarse(15))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n [✓] Desplazamiento táctico ejecutado con éxito.");
                        RegistrarEnBitacora($"Prueba de vuelo ejecutada en {naveActiva.Nombre}.");
                    }
                    else
                    {
                        MostrarError("La nave no posee suficiente combustible o no está operativa.");
                    }
                    Pausar();
                }
                else if (op == "3")
                {
                    if (naveActiva.AlmacenarRecurso(20))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n [✓] Minerales transferidos y almacenados en el contenedor de la nave.");
                        RegistrarEnBitacora($"Carga de minerales aumentada en {naveActiva.Nombre}.");
                    }
                    else
                    {
                        MostrarError("Capacidad máxima de carga alcanzada en esta nave.");
                    }
                    Pausar();
                }
                else if (op == "4")
                {
                    naveActiva.RecargarCombustible(30);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n [✓] Celdas energizadas. Combustible de la nave restaurado.");
                    RegistrarEnBitacora($"Combustible recargado en {naveActiva.Nombre}.");
                    Pausar();
                }
                else if (op == "5")
                {
                    naveActiva.RepararEscudos(30);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\n [✓] Matriz de plasma restaurada. Escudos de la nave reparados.");
                    RegistrarEnBitacora($"Escudos reparados en {naveActiva.Nombre}.");
                    Pausar();
                }
                else if (op == "6")
                {
                    enHangar = false;
                }
                else
                {
                    MostrarError("Opción no válida.");
                    Pausar();
                }
            }
        }

        static void ModuloEnlaceIris()
        {
            Console.Clear();
            DibujarBannerHeader();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n=== TERMINAL INTERACTIVA DE ENTIDAD IRIS ===\n");
            Console.ResetColor();

            irisConectado = true;
            Console.WriteLine($" [IRIS]: Saludos, Cadete {jugadorCadete.Nombre}. Enlace neuronal activo con NEXUS OS (2297).");
            Console.WriteLine(" [IRIS]: ¿En qué puedo colaborar en este ciclo de exploración?\n");

            Console.WriteLine(" [1] Consultar estado de directivas y Misión 'La anomalía perdida'");
            Console.WriteLine(" [2] Solicitar inyección de energía de emergencia (+15 HP Core)");
            Console.WriteLine(" [3] Modificar identificador del Cadete");
            Console.WriteLine(" [4] Cerrar canal de comunicación");
            Console.Write("\nDiálogo > ");

            string? op = Console.ReadLine()?.Trim();
            if (op == "1")
            {
                Console.WriteLine($"\n[IRIS]: Misión Recolección de Minerales: {(misionRecoleccionCompleta ? "COMPLETADA" : "PENDIENTE")}");
                Console.WriteLine($"[IRIS]: Misión 'La anomalía perdida':   {(misionAnomaliaCompletada ? "COMPLETADA" : "EN PROGRESO")}");
                Console.WriteLine($"[IRIS]: Enlace Red TCP:                 {(misionRedCompleta ? "COMPLETADA" : "PENDIENTE")}");
                RegistrarEnBitacora("Consulta de misiones realizada mediante IRIS.");
            }
            else if (op == "2")
            {
                jugadorCadete.Curar(15);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n[IRIS]: Protocolo de soporte ejecutado. HP del Core restaurado en +15.");
                RegistrarEnBitacora("Inyección energizante recibida de IRIS (+15 HP Core).");
            }
            else if (op == "3")
            {
                Console.Write("\nIngrese nuevo identificador de Cadete: ");
                string? nuevoNombre = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(nuevoNombre))
                {
                    jugadorCadete = new Cadete(nuevoNombre, jugadorCadete.PosX, jugadorCadete.PosY, jugadorCadete.Vida);
                    Console.WriteLine($"\n[IRIS]: Identificador actualizado a '{jugadorCadete.Nombre}'.");
                    RegistrarEnBitacora($"Cadete renombrado a {jugadorCadete.Nombre}.");
                }
            }
            Pausar();
        }

        static void ModuloConexionPuertoTCP()
        {
            Console.Clear();
            DibujarBannerHeader();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== MÓDULO DE RED TCP (NEXUS LINK 2297) ===\n");
            Console.ResetColor();

            Console.WriteLine($"Estado de Conexión: {(redConectada ? "CONECTADO" : "DESCONECTADO")}");
            Console.WriteLine($"Puerto Objetivo:   {puertoTCP}");
            Console.WriteLine($"IP Remota:         {ipRemota}\n");

            Console.WriteLine(" [1] Apertura y conexión de Socket TCP");
            Console.WriteLine(" [2] Transmitir paquete de telemetría del estado");
            Console.WriteLine(" [3] Configurar Puerto e IP de destino");
            Console.WriteLine(" [4] Desconectar Socket TCP");
            Console.WriteLine(" [5] Volver");
            Console.Write("\nSelección > ");

            string? op = Console.ReadLine()?.Trim();
            if (op == "1")
            {
                try
                {
                    Console.WriteLine("\n[RED] Conectando a servidor de enlace en el año 2297...");
                    socketClienteRed = new TcpClient();
                    var ar = socketClienteRed.BeginConnect(ipRemota, puertoTCP, null, null);
                    bool exito = ar.AsyncWaitHandle.WaitOne(1500);

                    if (exito && socketClienteRed.Connected)
                    {
                        streamRed = socketClienteRed.GetStream();
                        redConectada = true;
                        misionRedCompleta = true;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("[✓] ¡Enlace TCP establecido con éxito!");
                        RegistrarEnBitacora($"Conexión Socket TCP establecida en puerto {puertoTCP}.");
                    }
                    else
                    {
                        socketClienteRed.Close();
                        MostrarError($"Servidor no detectado en {ipRemota}:{puertoTCP}. Modo red no activo.");
                    }
                }
                catch (Exception ex)
                {
                    MostrarError($"Falla al abrir puerto TCP: {ex.Message}");
                }
                Pausar();
            }
            else if (op == "2")
            {
                if (redConectada && streamRed != null)
                {
                    try
                    {
                        string data = $"CADETE:{jugadorCadete.Nombre}|HP:{jugadorCadete.Vida}|ENERGIA:{energiaCore}|NAVE:{hangarNaves[naveSeleccionadaIndex].Nombre}";
                        byte[] buffer = Encoding.UTF8.GetBytes(data);
                        streamRed.Write(buffer, 0, buffer.Length);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\n[✓] Paquete de telemetría transmitido por el puerto TCP.");
                        RegistrarEnBitacora("Paquete de telemetría enviado vía TCP.");
                    }
                    catch (Exception ex)
                    {
                        MostrarError($"Error transmitiendo datos: {ex.Message}");
                    }
                }
                else
                {
                    MostrarError("No hay una conexión TCP activa para transmitir datos.");
                }
                Pausar();
            }
            else if (op == "3")
            {
                Console.Write("\nIngrese nueva IP de destino: ");
                string? ip = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(ip)) ipRemota = ip;

                Console.Write("Ingrese nuevo Puerto TCP: ");
                if (int.TryParse(Console.ReadLine(), out int p)) puertoTCP = p;

                RegistrarEnBitacora($"Parámetros de red actualizados a {ipRemota}:{puertoTCP}");
                Pausar();
            }
            else if (op == "4")
            {
                CerrarConexionRed();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n[!] Enlace TCP finalizado.");
                Pausar();
            }
        }

        static void ModuloJuegoModo2D()
        {
            bool jugando = true;

            while (jugando && jugadorCadete.Vida > 0)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"=== SIMULACIÓN 2D EN TIEMPO REAL [{nombreSectorActivo}] ===");
                Console.ResetColor();
                Console.WriteLine($"Cadete: {jugadorCadete.Nombre} | HP Core: {jugadorCadete.Vida}% | Energía Core: {energiaCore}% | Minerales: {mineralesRecolectados} | Llave: {(tieneLlave ? "SÍ" : "NO")}\n");

                int filas = mapaActivo.GetLength(0);
                int columnas = mapaActivo.GetLength(1);

                for (int y = 0; y < filas; y++)
                {
                    for (int x = 0; x < columnas; x++)
                    {
                        if (x == playerX && y == playerY)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.Write("@ ");
                            Console.ResetColor();
                            continue;
                        }

                        bool esEntidad = false;
                        foreach (var ent in listaEntidadesActivas)
                        {
                            if (ent.EstaVivo && ent.PosX == x && ent.PosY == y)
                            {
                                if (ent is Robot) Console.ForegroundColor = ConsoleColor.Yellow;
                                else Console.ForegroundColor = ConsoleColor.Red;

                                Console.Write("E ");
                                Console.ResetColor();
                                esEntidad = true;
                                break;
                            }
                        }

                        if (esEntidad) continue;

                        int celda = mapaActivo[y, x];
                        switch (celda)
                        {
                            case 1:
                                Console.ForegroundColor = ConsoleColor.DarkGray;
                                Console.Write("# ");
                                break;
                            case 2:
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.Write("* ");
                                break;
                            case 3:
                                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                                Console.Write("▲ ");
                                break;
                            case 4:
                                Console.ForegroundColor = ConsoleColor.Cyan;
                                Console.Write("K ");
                                break;
                            case 5:
                                Console.ForegroundColor = ConsoleColor.Blue;
                                Console.Write("D ");
                                break;
                            default:
                                Console.Write(". ");
                                break;
                        }
                        Console.ResetColor();
                    }
                    Console.WriteLine();
                }

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("\n [WASD / Flechas] Moverse  │  [Q] Salir de la matriz");
                Console.ResetColor();

                ConsoleKeyInfo key = Console.ReadKey(true);
                int nextX = playerX;
                int nextY = playerY;

                if (key.Key == ConsoleKey.W || key.Key == ConsoleKey.UpArrow) nextY--;
                else if (key.Key == ConsoleKey.S || key.Key == ConsoleKey.DownArrow) nextY++;
                else if (key.Key == ConsoleKey.A || key.Key == ConsoleKey.LeftArrow) nextX--;
                else if (key.Key == ConsoleKey.D || key.Key == ConsoleKey.RightArrow) nextX++;
                else if (key.Key == ConsoleKey.Q)
                {
                    jugando = false;
                    break;
                }

                if (nextX >= 0 && nextX < columnas && nextY >= 0 && nextY < filas)
                {
                    int casillaFutura = mapaActivo[nextY, nextX];

                    if (casillaFutura != 1)
                    {
                        if (casillaFutura == 5 && !tieneLlave)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n[!] La puerta está cerrada. Necesitas encontrar la llave ('K').");
                            Console.ResetColor();
                            Thread.Sleep(800);
                        }
                        else
                        {
                            playerX = nextX;
                            playerY = nextY;

                            if (casillaFutura == 2)
                            {
                                mineralesRecolectados += 15;
                                mapaActivo[playerY, playerX] = 0;
                                RegistrarEnBitacora("Mineral recolectado en simulación 2D (+15).");
                            }
                            else if (casillaFutura == 3)
                            {
                                jugadorCadete.RecibirDano(15);
                                RegistrarEnBitacora("Daño recibido por trampa en simulación 2D (-15 HP).");
                            }
                            else if (casillaFutura == 4)
                            {
                                tieneLlave = true;
                                mapaActivo[playerY, playerX] = 0;
                                RegistrarEnBitacora("Llave de acceso obtenida.");
                            }
                            else if (casillaFutura == 5 && tieneLlave)
                            {
                                misionInfiltracionCompleta = true;
                                mapaActivo[playerY, playerX] = 0;
                                RegistrarEnBitacora("Infiltración exitosa. Puerta desbloqueada.");
                            }
                        }
                    }
                }
                else
                {
                    // CONTROL DE Moverse fuera del mapa
                    MostrarError("Límite de mapa alcanzado. No puedes desplazarte fuera de los bordes.");
                    Thread.Sleep(600);
                }

                foreach (var ent in listaEntidadesActivas)
                {
                    if (ent.EstaVivo && ent is Enemigo enemigo)
                    {
                        enemigo.MoverHacia(playerX, playerY, mapaActivo);

                        if (enemigo.PosX == playerX && enemigo.PosY == playerY)
                        {
                            jugadorCadete.RecibirDano(enemigo.DanoAtaque);
                            RegistrarEnBitacora($"Emboscada de {enemigo.Nombre}! HP reducido en -{enemigo.DanoAtaque}.");
                        }
                    }
                }

                VerificarMisiones();
            }

            if (jugadorCadete.Vida <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n[CRÍTICO] El Cadete ha caído en combate. Reiniciando subsistemas...");
                Console.ResetColor();
                Pausar();
            }
        }

        static void ModuloSeleccionarMapa()
        {
            Console.Clear();
            DibujarBannerHeader();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n=== TRANSICIÓN DE REALIDADES Y SECTORES DIMENSIONALES ===\n");
            Console.ResetColor();

            Console.WriteLine($"Sector Actual: {nombreSectorActivo}\n");
            Console.WriteLine(" [1] Realidad Alfa (Superficie Anómala)");
            Console.WriteLine(" [2] Realidad Beta (Cavernas Subterráneas)");
            Console.WriteLine(" [3] Realidad Gamma (Anomalía Cuántica)");
            Console.WriteLine(" [4] Realidad Delta (Estación Espacial Búnker)");
            Console.WriteLine(" [5] Cancelar salto dimensional");
            Console.Write("\nSeleccione destino > ");

            string? op = Console.ReadLine()?.Trim();
            if (op == "1")
            {
                mapaActivo = mapaAlfa;
                nombreSectorActivo = "Realidad Alfa (Superficie Anómala)";
                listaEntidadesActivas = listaEntidadesAlfa;
                playerX = 1; playerY = 1;
                RegistrarEnBitacora("Salto dimensional hacia Realidad Alfa.");
            }
            else if (op == "2")
            {
                mapaActivo = mapaBeta;
                nombreSectorActivo = "Realidad Beta (Cavernas Subterráneas)";
                listaEntidadesActivas = listaEntidadesBeta;
                playerX = 1; playerY = 1;
                RegistrarEnBitacora("Salto dimensional hacia Realidad Beta.");
            }
            else if (op == "3")
            {
                mapaActivo = mapaGamma;
                nombreSectorActivo = "Realidad Gamma (Anomalía Cuántica)";
                listaEntidadesActivas = listaEntidadesGamma;
                playerX = 1; playerY = 1;
                RegistrarEnBitacora("Salto dimensional hacia Realidad Gamma.");
            }
            else if (op == "4")
            {
                mapaActivo = mapaDelta;
                nombreSectorActivo = "Realidad Delta (Estación Espacial Búnker)";
                listaEntidadesActivas = listaEntidadesDelta;
                playerX = 1; playerY = 1;
                RegistrarEnBitacora("Salto dimensional hacia Realidad Delta.");
            }
            else
            {
                Console.WriteLine("\nTransición cancelada.");
                Pausar();
            }
        }

        // =========================================================================
        // MÉTODOS AUXILIARES Y CONTROL DE ERRORES / SITUACIONES
        // =========================================================================
        static void DibujarBannerHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($" NEXUS ENGINE 2297 ── [CADETE: {jugadorCadete.Nombre.ToUpper()}] ── [HP: {jugadorCadete.Vida}/{jugadorCadete.VidaMax}] ── [ENERGÍA: {energiaCore}%]");
            Console.ResetColor();
        }

        static void RegistrarEnBitacora(string mensaje)
        {
            string registro = $"[{DateTime.Now:HH:mm:ss}] {mensaje}";
            bitacoraRegistros.Add(registro);
        }

        static void VerificarMisiones()
        {
            if (mineralesRecolectados >= 100) misionRecoleccionCompleta = true;
            if (redConectada) misionRedCompleta = true;
        }

        static void CerrarConexionRed()
        {
            if (streamRed != null)
            {
                streamRed.Close();
                streamRed = null;
            }
            if (socketClienteRed != null)
            {
                socketClienteRed.Close();
                socketClienteRed = null;
            }
            redConectada = false;
        }

        static void MostrarError(string mensaje)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n[ERROR / ALERTA DE CONTROL]: {mensaje}");
            Console.ResetColor();
        }

        static void Pausar()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ResetColor();
            Console.ReadKey(true);
        }
    }
}