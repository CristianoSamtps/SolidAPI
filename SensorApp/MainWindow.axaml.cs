using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using SensorApp.Models;

namespace SensorApp;

public partial class MainWindow : Window
{
    // Configuração
    private const string BaseUrl = "http://localhost:8765/api/solid";
    private const string AppName = "moisture-sensor";
    private const string ChannelName = "humidity";
    private const int MinutosAteExpirar = 10;   

    private readonly SolidService _solid;
    private readonly DispatcherTimer _timer;
    private readonly Random _random;

    private int _humidade;
    private bool _registado = false;

    public MainWindow()
    {
        InitializeComponent();

        _solid = new SolidService(BaseUrl);
        _random = new Random();
        _humidade = _random.Next(30, 71);   // começa num valor "normal"

        // Relógio que "dispara" de segundo a segundo
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += Timer_Tick;
    }

    // Botão "Ligar": regista-se no SOLID (só da primeira vez) e começa a medir
    private async void BtnLigar_Click(object? sender, RoutedEventArgs e)
    {
        btnLigar.IsEnabled = false;
        txtStatus.Text = "A ligar ao SOLID...";

        if (!_registado)
        {
            _registado = await Registar();
        }

        if (_registado)
        {
            _timer.Start();
            btnDesligar.IsEnabled = true;
            txtStatus.Text = $"Ligado - a publicar em {AppName}/{ChannelName}";
        }
        else
        {
            btnLigar.IsEnabled = true;
            txtStatus.Text = "Não foi possível ligar ao SOLID. A API está ligada?";
        }
    }

    // Botão "Desligar": pára de medir
    private void BtnDesligar_Click(object? sender, RoutedEventArgs e)
    {
        _timer.Stop();
        btnLigar.IsEnabled = true;
        btnDesligar.IsEnabled = false;
        txtStatus.Text = "Sensor desligado";
    }

    // Cria a application e o channel no SOLID, se ainda não existirem
    private async Task<bool> Registar()
    {
        try
        {
            if (!await _solid.Exists(AppName))
            {
                ResourceRequest application = new ResourceRequest
                {
                    ResType = "application",
                    ResourceName = AppName,
                    ResourceDesc = "Sensor de humidade do solo (simulado)"
                };
                await _solid.Create("", application);
                Escrever($"Application '{AppName}' criada.");
            }

            if (!await _solid.Exists($"{AppName}/{ChannelName}"))
            {
                ResourceRequest channel = new ResourceRequest
                {
                    ResType = "channel",
                    ResourceName = ChannelName,
                    ResourceDesc = "Medições de humidade do solo, em %"
                };
                await _solid.Create(AppName, channel);
                Escrever($"Channel '{ChannelName}' criado.");
            }

            return true;
        }
        catch (Exception ex)
        {
            Escrever($"Erro ao registar no SOLID: {ex.Message}");
            return false;
        }
    }

    // Acontece de segundo a segundo, enquanto o sensor está ligado
    private async void Timer_Tick(object? sender, EventArgs e)
    {
        // A humidade muda um pouco de cada vez (entre -5 e +5), como num solo verdadeiro
        _humidade = _humidade + _random.Next(-5, 6);
        if (_humidade < 0)
        {
            _humidade = 0;
        }
        if (_humidade > 100)
        {
            _humidade = 100;
        }

        txtHumidade.Text = $"{_humidade} %";

        // Sem ResourceName: o SOLID inventa um nome único e devolve-o na resposta
        ResourceRequest dataRecord = new ResourceRequest
        {
            ResType = "data-record",
            ContentType = "text/plain",
            Content = _humidade.ToString(),
            DtExpiration = DateTime.Now.AddMinutes(MinutosAteExpirar).ToString("yyyy-MM-ddTHH:mm:ss")
        };

        try
        {
            ResourceRequest? criado = await _solid.Create($"{AppName}/{ChannelName}", dataRecord);
            Escrever($"{_humidade}% -> publicado como {criado?.ResourceName}");
        }
        catch (Exception ex)
        {
            Escrever($"{_humidade}% -> erro: {ex.Message}");
        }
    }

    // Escreve uma linha no histórico (a mais recente fica em cima)
    private void Escrever(string mensagem)
    {
        string hora = DateTime.Now.ToString("HH:mm:ss");
        txtMessages.Text = $"[{hora}] {mensagem}\n" + txtMessages.Text;
    }
}
