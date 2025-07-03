using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Linq;

public class Aplicacao : Form
{
    private Panel frameBotoes;
    private Panel frameAtivas;
    private Panel frameFila;
    private Panel frameConcluidas;

    private Label labelAtivas;
    private Label labelFila;
    private Label labelConcluidas;
    private Label lbMemoria;

    private Button botaoPaginacao;
    private Button botaoParar;
    private Button botaoOverlay;

    private Dictionary<int,(string nome, int duracao)> rotinasAtivas = new Dictionary<int,(string nome, int duracao)>();
    private List<string> filaRotinas = new List<string>();
    private List<string> rotinasConcluidas = new List<string>();
    private Dictionary<string, Label> labelsTempo = new Dictionary<string, Label>();
    private Dictionary<string, int> tempoExecucao = new Dictionary<string, int>();

    private System.Windows.Forms.Timer atualizador = new System.Windows.Forms.Timer();

    private bool isPaginacao;
    private bool havePrincipalRoutine;
    

    public Aplicacao()
    {
        InicializarComponentes();
        ConfigurarSubrotinas();
        AtualizarLabels(null, null);
        atualizador.Interval = 500;
        int tempoDecorrido = 0;
        atualizador.Tick += AtualizarLabels;
        atualizador.Start();
    }

    private void InicializarComponentes()
    {
        this.Text = "Simulador Paginação"; //titulo
        this.BackColor = Color.DarkBlue; //cor do fundo
        this.Size = new Size(1100, 600); //tamanho da janela
        this.MinimumSize = new Size(1100, 600); //faz a janela ser aumentavel
        this.MaximumSize = new Size(1920, 1080);

        lbMemoria = new Label()
        {
            Text = "Simulador de Paginação", //texto
            ForeColor = Color.Yellow, //cor da letra
            BackColor = Color.DarkBlue,
            Font = new Font("Helvetica", 15), //fonte e tamanho
            AutoSize = true,
            Location = new Point(400, 30) //local onde fica
        };
        this.Controls.Add(lbMemoria);

        frameBotoes = new Panel() //parte onde fica os botoes
        {
            Size = new Size(300, 100), //tamanho
            Location = new Point(360, 90), //local
            BackColor = Color.DarkBlue //cor de fundo
        };
        this.Controls.Add(frameBotoes);

        botaoPaginacao = new Button() //botao
        {
            Text = "Paginacão", //texto do botao
            Font = new Font("Helvetica", 12), //fonte e tamanho
            Size = new Size(100, 30), //tamanho
            Location = new Point(160, 10), //localização
            ForeColor = Color.Yellow
        };
        botaoPaginacao.Click += Paginacao;
        frameBotoes.Controls.Add(botaoPaginacao);

        botaoOverlay = new Button() //botão de overlay
        {
            Text = "Overlay",
            Font = new Font("Helvetica", 12),
            Size = new Size(100, 30),
            Location = new Point(45, 10),
            ForeColor = Color.Yellow
        };
        botaoOverlay.Click += Overlay;
        frameBotoes.Controls.Add(botaoOverlay);

        botaoParar = new Button() //botão de parar
        {
            Text = "Parar",
            Font = new Font("Helvetica", 12),
            Size = new Size(100, 30),
            Location = new Point(100, 50),
            ForeColor = Color.Yellow
        };
        botaoParar.Click += Parar;
        frameBotoes.Controls.Add(botaoParar);

        frameAtivas = CriarPainelLista("Ativas:", //painel de rotinas ativas
            Color.FromArgb(1,200,200), 
            new Point(375, 200), 
            out labelAtivas);

        frameFila = CriarPainelLista("Fila:",  //painel de rotinas na fila
            Color.FromArgb(200,200,200), 
            new Point(100, 200),
            out labelFila);

        frameConcluidas = CriarPainelLista("Concluídas:", //painel de rotinas concluidas
            Color.FromArgb(102,254,95),
            new Point(650, 200),
            out labelConcluidas);
    }

    private Panel CriarPainelLista(string titulo, Color bgColor, Point local, out Label label)
    {
        Panel panel = new Panel()
        {
            Size = new Size(250, 300), //tamanho
            Location = local, //localização
            BackColor = bgColor, //cor de fundo
            BorderStyle = BorderStyle.Fixed3D //tipo da borda
        };
        label = new Label()
        {
            Text = titulo, //texto do panel
            Font = new Font("Helvetica", 12), //fonte do texto
            AutoSize = false, //se se redimensiona
            TextAlign = ContentAlignment.TopLeft, //alinhamento
            Dock = DockStyle.Fill //a maneira de encaixe
        };
        panel.Controls.Add(label);
        this.Controls.Add(panel);
        return panel;
    }

    private void ConfigurarSubrotinas()
    {
        filaRotinas.Clear(); //libera a fila
        tempoExecucao.Clear();
        Random rand = new Random();
        for (int i = 1; i <= rand.Next(10,20); i++) //cria de 10 a 20 rotinas
        {
            if(!havePrincipalRoutine && !isPaginacao)
            {
                string nome = $"Rotina Principal";
                filaRotinas.Add(nome);
                tempoExecucao[nome] = 10000;
                havePrincipalRoutine = true;
            }

            if (isPaginacao)
            {
                string nome = $"Rotina {i}";
                filaRotinas.Add(nome);
                tempoExecucao[nome] = rand.Next(5000, 15000);
            }
            else
            {
                string nome = $"Subrotina {i}"; //nome da rotina
                filaRotinas.Add(nome); //adiciona a fila
                tempoExecucao[nome] = rand.Next(8000, 20000); //o tempo de execução dela em milisegundos
            }
        }
    }

    private void AtualizarLabels(object sender, EventArgs e)
    {
        var copiaAtivas = rotinasAtivas.ToList();

        labelAtivas.Text = "Ativas:\n" + string.Join("\n",
            copiaAtivas.Select(kv => $"{kv.Value.nome} – {kv.Value.duracao}s"));
        labelFila.Text = "Fila:\n" + string.Join("\n", filaRotinas.GetRange(0, Math.Min(10, filaRotinas.Count)));
        labelConcluidas.Text = "Concluídas:\n" + string.Join("\n", rotinasConcluidas);
    }

    private CancellationTokenSource cancelamento;
    private Task rotinaPrincipal;
    private Random random = new Random();

    private void InvocarAtualizacao()
    {
        if (InvokeRequired)
            Invoke(new Action(() => AtualizarLabels(null,null)));
        else
            AtualizarLabels(null, null);
    }

    private async Task ExecutarSubrotina(int id, string nome, CancellationToken token, DateTime fim)
    {
        int tempo = tempoExecucao[nome]; // Tempo total em milissegundos
        int tempoRestante = tempo / 1000; // Converter para segundos

        rotinasAtivas[id] = (nome, tempoRestante); //atualiza a rotina
        InvocarAtualizacao();

        for(int i = tempoRestante; i >= 0; i--)
        {
            if (token.IsCancellationRequested || DateTime.Now > fim) return; //cancela quando aperta em parar ou acabar o tempo


            rotinasAtivas[id] = (nome, i); //atualiza o tempo de execução
            InvocarAtualizacao();

            await Task.Delay(1000); //espera 1 segundo para iterar no loop novamente
        }

        if(nome == "Rotina Principal" && !isPaginacao)
        {
            await EsperarCondicaoAsync(() => rotinasAtivas.Count == 1); //espera as outras sub rotinas terminarem
        }

        rotinasAtivas.Remove(id); //tira da aba de ativas
        rotinasConcluidas.Add(nome); //coloca na aba de concluidas

        InvocarAtualizacao(); //atualiza os panes     

        // Simula a criação de uma nova sub-rotina com 30% de chance
        if (random.NextDouble() < 0.3 && isPaginacao)
        {
            string nova = $"Nova Rotina {random.Next(1, 100)}";
            tempoExecucao[nova] = random.Next(3000, 10000); //define um tempo de 3 a 10 segundos
            filaRotinas.Add(nova); //adiciona para a fila
        }
    }

    private void Iniciar(object sender, EventArgs e)
    {
        Parar(null, null); //permite que o programa inicie do zero

        cancelamento = new CancellationTokenSource();
        CancellationToken token = cancelamento.Token;

        List<Task> subRotinas = new List<Task>();

        rotinaPrincipal = Task.Run(async () =>
        {
            DateTime fim;

            if (!isPaginacao)
            {
                fim = DateTime.Now.AddSeconds(60); // duração da rotina principal
            }
            else
            {
                fim = DateTime.Now.AddSeconds(30);
            }
                ConfigurarSubrotinas(); //cria 15 sub rotinas com duração aleatoria

            while (DateTime.Now < fim && !token.IsCancellationRequested)
            {
                if (filaRotinas.Count > 0) //enquanto tiver rotinas
                {
                    string subrotina = filaRotinas[0]; //pega a primeira da lista
                    filaRotinas.RemoveAt(0); //remove da fila para colocar na ativa

                    int id = Environment.TickCount;
                    int tempoSegundos = tempoExecucao[subrotina] / 1000;
                    rotinasAtivas[id] = (subrotina, tempoSegundos); //adiciona a sub rotina a lista de ativas

                    InvocarAtualizacao(); //atualiza os panes

                    _ = Task.Run(async () => //aqui define que vai ser rodado as rotinas enquanto roda a execução do programa
                    {
                        await ExecutarSubrotina(id, subrotina, token, fim);
                    });
                }

                await Task.Delay(1000); //espera 1 segundo entre as rotinas
            }

            // Após terminar, limpa a lista de ativas
            rotinasAtivas.Clear();
            rotinasConcluidas.Clear();
            filaRotinas.Clear();
            InvocarAtualizacao();
            havePrincipalRoutine = false;
        }, token);
    }

    private void Parar(object sender, EventArgs e)
    {
        //da clear em tudo
        cancelamento?.Cancel();
        rotinasAtivas.Clear();
        rotinasConcluidas.Clear();
        filaRotinas.Clear();
        labelsTempo.Clear();
        tempoExecucao.Clear();
        InvocarAtualizacao();
        havePrincipalRoutine = false;
    }

    private void Overlay(object sender, EventArgs e)
    {
        isPaginacao = false;
        Iniciar(null, null);
    }

    private void Paginacao(Object sender, EventArgs e)
    {
        isPaginacao = true;
        Iniciar(null, null);
    }

    private async Task EsperarCondicaoAsync(Func<bool> condicao, int checagemMs = 100)
    {
        while (!condicao())
        {
            await Task.Delay(checagemMs);
        }
    }

    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new Aplicacao());
    }

    private void InitializeComponent()
    {
            this.SuspendLayout();
            // 
            // Aplicacao
            // 
            this.ClientSize = new System.Drawing.Size(831, 461);
            this.Name = "Aplicacao";
            this.ResumeLayout(false);

    }
}
