using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    // HEADER MENUS
    public GameObject menuInicio;
    public GameObject panelPrincipal;      
    public GameObject panelJugar;          
    public GameObject panelRecords;
    public GameObject menuGameOver;
    public GameObject menuPausa;

    // HEADER UI
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI recordText;
    public TextMeshProUGUI recordTextMenu;
    public TextMeshProUGUI recordTextGameOver;
    public TMP_InputField nombreInput;

    // HEADER JUEGO
    public GameObject pinguino;
    public float velocidad = 2;
    public GameObject Columna;
    public Renderer fondo;
    public GameObject piedra1;
    public GameObject piedra2;

    // HEADER PERSONAJES
    public Sprite[] personajesSprites;
    public RuntimeAnimatorController[] personajesAnimators;
    public UnityEngine.UI.Image personajePreview; 
    private int personajeSeleccionado = 0;

    // HEADER ESTADO
    public bool gameOver = false;
    public bool start = false;
    public bool pausa = false;

    // HEADER LISTAS
    public List<GameObject> cols = new List<GameObject>();
    public List<GameObject> obstaculos = new List<GameObject>();

    private float distance = 0;
    private string nombreJugador = "";

    void Start()
    {
        if (pinguino != null)
            pinguino.SetActive(false);

        // Inicializar el texto de distancia
        distance = 0;
        if (distanceText != null)
        {
            distanceText.text = $"{distance.ToString("0000")}";
            distanceText.gameObject.SetActive(false);
        }
        recordText.text = "";

        // Crear Mapa
        for (int i = 0; i < 21; i++)
        {
            cols.Add(Instantiate(Columna, new Vector2(-10 + i, -3), Quaternion.identity));
        }

        // Mostrar menú principal al inicio
        MostrarMenuPrincipal();
    }

    void Update()
    {
        // Esperar nombre antes de empezar
        if (!start && !gameOver)
        {
            if (panelJugar != null && panelJugar.activeSelf)
            {
                // Cambiar personaje con flechas izquierda/derecha
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    personajeSeleccionado--;
                    if (personajeSeleccionado < 0)
                        personajeSeleccionado = personajesSprites.Length - 1;
                    ActualizarVistaPersonaje();
                }

                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    personajeSeleccionado++;
                    if (personajeSeleccionado >= personajesSprites.Length)
                        personajeSeleccionado = 0;
                    ActualizarVistaPersonaje();
                }

                // Iniciar juego con Enter
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    IniciarJuego();
                }
            }
            return;
        }

        // GameOver
        if (start && gameOver)
        {
            if (!menuGameOver.activeSelf) // Solo ejecutar una vez
            {
                menuGameOver.SetActive(true);
                if (recordTextGameOver != null)
                {
                    MostrarPosicionEnRanking(recordTextGameOver);
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            return;
        }

        // Pausa
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!pausa)
            {
                pausa = true;
                Time.timeScale = 0;
                menuPausa.SetActive(true);
            }
            else
            {
                pausa = false;
                Time.timeScale = 1;
                menuPausa.SetActive(false);
            }
        }

        // Juego en curso
        if (start && !gameOver)
        {
            fondo.material.mainTextureOffset = fondo.material.mainTextureOffset + new Vector2(0.025f, 0) * Time.deltaTime;

            // Incrementar la distancia
            distance += velocidad * Time.deltaTime;
            distanceText.text = $"{distance.ToString("0000")}";

            // Aumento de dificultad (velocidad)
            velocidad = 2 + (distance / 200f);

            // Mover mapa
            for (int i = 0; i < cols.Count; i++)
            {
                if (cols[i].transform.position.x <= -10)
                {
                    cols[i].transform.position = new Vector3(10, -3, 0);
                }

                cols[i].transform.position = cols[i].transform.position + new Vector3(-1, 0, 0) * Time.deltaTime * velocidad;
            }

            // Mover obstaculos
            for (int i = 0; i < obstaculos.Count; i++)
            {
                if (obstaculos[i].transform.position.x <= -10)
                {
                    float randomObs = Random.Range(10f, 13f);
                    GameObject nuevaRoca = Random.Range(0, 2) == 0 ? piedra1 : piedra2;

                    Destroy(obstaculos[i]);
                    obstaculos[i] = Instantiate(nuevaRoca, new Vector2(randomObs, -2), Quaternion.identity);
                }

                obstaculos[i].transform.position = obstaculos[i].transform.position + new Vector3(-1, 0, 0) * Time.deltaTime * velocidad;
            }
        }
    }

    // MENU
    public void MostrarMenuPrincipal()
    {
        menuInicio.SetActive(true);
        panelPrincipal.SetActive(true);
        panelJugar.SetActive(false);
        panelRecords.SetActive(false);
        
        // Mostrar records en el menú principal
        if (recordTextMenu != null)
            MostrarRecordsEnTexto(recordTextMenu);
    }

    public void MostrarPanelJugar()
    {
        panelPrincipal.SetActive(false);
        panelJugar.SetActive(true);
        panelRecords.SetActive(false);
        
        // Inicializar vista de personaje
        ActualizarVistaPersonaje();
        
        // Focus en el input
        if (nombreInput != null)
        {
            nombreInput.Select();
            // Agregar evento para detectar Enter
            nombreInput.onSubmit.RemoveAllListeners();
            nombreInput.onSubmit.AddListener((string texto) => IniciarJuego());
        }
    }

    public void MostrarPanelRecords()
    {
        panelPrincipal.SetActive(false);
        panelJugar.SetActive(false);
        panelRecords.SetActive(true);
        
        // Mostrar records completos
        if (recordText != null)
            MostrarRecordsEnTexto(recordText);
    }

    public void VolverAlMenuPrincipal()
    {
        MostrarMenuPrincipal();
    }

    private void IniciarJuego()
    {
        if (nombreInput.text.Length > 0)
        {
            nombreJugador = nombreInput.text;
            start = true;
            menuInicio.SetActive(false);

            if (distanceText != null)
            {
                distanceText.gameObject.SetActive(true);
            }

            if (pinguino != null)
            {
                pinguino.SetActive(true);

                // Cambiar sprite del personaje
                SpriteRenderer spriteRenderer = pinguino.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && personajesSprites.Length > personajeSeleccionado)
                {
                    spriteRenderer.sprite = personajesSprites[personajeSeleccionado];
                }

                // Cambiar animator del personaje
                Animator animator = pinguino.GetComponent<Animator>();
                if (animator != null && personajesAnimators.Length > personajeSeleccionado)
                {
                    animator.runtimeAnimatorController = personajesAnimators[personajeSeleccionado];
                }
            }
            GenerarRocaInicial();
        }
    }
    
    private void GenerarRocaInicial()
    {
        for (int i = 0; i < 5; i++)
        {
            float posX = 8 + (i * Random.Range(2f, 4f));
            GameObject tipoRoca = Random.Range(0, 2) == 0 ? piedra1 : piedra2;
            obstaculos.Add(Instantiate(tipoRoca, new Vector2(posX, -2), Quaternion.identity));
        }
    }

    private void ActualizarVistaPersonaje()
    {
        if (personajePreview != null && personajesSprites.Length > 0)
        {
            personajePreview.sprite = personajesSprites[personajeSeleccionado];
        }
    }

    // RECORDS

    public void GuardarRecord()
    {
        if (string.IsNullOrEmpty(nombreJugador)) return;

        List<Record> records = CargarRecords();
        records.Add(new Record(nombreJugador, distance));

        records.Sort((a, b) => b.puntaje.CompareTo(a.puntaje));
        if (records.Count > 5)
            records = records.GetRange(0, 5);

        for (int i = 0; i < records.Count; i++)
        {
            PlayerPrefs.SetString($"RecordName{i}", records[i].nombre);
            PlayerPrefs.SetFloat($"RecordScore{i}", records[i].puntaje);
        }
        PlayerPrefs.Save();
    }

    private void MostrarRecordsEnTexto(TextMeshProUGUI texto)
    {
        List<Record> records = CargarRecords();
        texto.text = "Top 5 Records\n";

        if (records.Count == 0)
        {
            texto.text += "No hay records aún.\n¡Sé el primero!";
        }
        else
        {
            for (int i = 0; i < records.Count; i++)
            {
                texto.text += $"{i + 1}. {records[i].nombre} - {records[i].puntaje.ToString("0000")}\n";
            }
        }
    }


    private List<Record> CargarRecords()
    {
        List<Record> records = new List<Record>();
        for (int i = 0; i < 5; i++)
        {
            if (PlayerPrefs.HasKey($"RecordName{i}"))
            {
                string n = PlayerPrefs.GetString($"RecordName{i}");
                float p = PlayerPrefs.GetFloat($"RecordScore{i}");
                records.Add(new Record(n, p));
            }
        }
        return records;
    }
    
    private void MostrarPosicionEnRanking(TextMeshProUGUI texto)
    {
        List<Record> records = CargarRecords();
        
        // Encontrar la posición del jugador actual
        int posicion = -1;
        for (int i = 0; i < records.Count; i++)
        {
            if (records[i].nombre == nombreJugador && records[i].puntaje == distance)
            {
                posicion = i + 1;
                break;
            }
        }
        
        if (posicion != -1)
        {
            texto.text = $"¡Quedaste en el puesto #{posicion}!\n\nTu puntaje: {distance.ToString("0000")}";
        }
        else
        {
            texto.text = $"Tu puntaje: {distance.ToString("0000")}\n\nNo entraste al Top 5";
        }
    }

}

[System.Serializable]
public class Record
{
    public string nombre;
    public float puntaje;
    public Record(string nombre, float puntaje)
    {
        this.nombre = nombre;
        this.puntaje = puntaje;
    }
}