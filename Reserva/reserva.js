const CONFIG = {
    whatsapp: '5511999999999' 
};


const HORARIOS = {
    0: [11, 22],
    1: [11, 23],
    2: [11, 23],
    3: [11, 23],
    4: [11, 23],
    5: [11, 24],
    6: [11, 24]
};


const DIAS = ['domingo', 'segunda-feira', 'terça-feira', 'quarta-feira',
              'quinta-feira', 'sexta-feira', 'sábado'];


const header = document.querySelector('.header');
const btnMenu = document.getElementById('btnMenu');

btnMenu.addEventListener('click', function () {
    const aberto = header.classList.toggle('aberto');
    btnMenu.setAttribute('aria-expanded', aberto);
});

const form = document.getElementById('formReserva');
const campoNome = document.getElementById('nome');
const campoTel = document.getElementById('telefone');
const campoData = document.getElementById('data');
const campoHora = document.getElementById('hora');
const campoPessoas = document.getElementById('pessoas');
const aviso = document.getElementById('aviso');

function avisar(texto, tipo) {
    aviso.textContent = texto;
    aviso.className = 'aviso ' + tipo;
}

function doisDigitos(n) {
    return String(n).padStart(2, '0');
}

function formatarISO(d) {
    return d.getFullYear() + '-' + doisDigitos(d.getMonth() + 1) + '-' + doisDigitos(d.getDate());
}


function lerData(valor) {
    const partes = valor.split('-').map(Number);
    return new Date(partes[0], partes[1] - 1, partes[2]);
}


try {
    const logado = JSON.parse(localStorage.getItem('usuarioLogado'));
    if (logado && logado.nome) campoNome.value = logado.nome;
} catch (e) {}


for (let n = 1; n <= 12; n++) {
    const opcao = document.createElement('option');
    opcao.value = n === 1 ? '1 pessoa' : n + ' pessoas';
    opcao.textContent = opcao.value;
    if (n === 2) opcao.selected = true;
    campoPessoas.appendChild(opcao);
}


const hoje = new Date();
const limite = new Date();
limite.setDate(limite.getDate() + 60);
campoData.min = formatarISO(hoje);
campoData.max = formatarISO(limite);


function montarHorarios() {
    campoHora.innerHTML = '';

    if (!campoData.value) {
        campoHora.innerHTML = '<option value="">Escolha a data primeiro</option>';
        campoHora.disabled = true;
        return;
    }

    const data = lerData(campoData.value);
    const faixa = HORARIOS[data.getDay()];
    const agora = new Date();
    const ehHoje = formatarISO(agora) === campoData.value;
    const minutosAgora = agora.getHours() * 60 + agora.getMinutes();

    const horarios = [];
    for (let min = faixa[0] * 60; min <= (faixa[1] - 1) * 60; min += 30) {
        if (ehHoje && min <= minutosAgora + 30) continue;
        horarios.push(doisDigitos(Math.floor(min / 60)) + ':' + doisDigitos(min % 60));
    }

    if (horarios.length === 0) {
        campoHora.innerHTML = '<option value="">Sem horários nesta data</option>';
        campoHora.disabled = true;
        avisar('Não há mais horários nesse dia. Escolha outra data.', 'erro');
        return;
    }

    campoHora.disabled = false;
    campoHora.innerHTML = '<option value="">Escolha o horário</option>' +
        horarios.map(function (h) { return '<option>' + h + '</option>'; }).join('');
    avisar('', '');
}

campoData.addEventListener('change', montarHorarios);
montarHorarios();


campoTel.addEventListener('input', function () {
    const d = campoTel.value.replace(/\D/g, '').slice(0, 11);
    let texto = d;

    if (d.length > 10) {
        texto = '(' + d.slice(0, 2) + ') ' + d.slice(2, 7) + '-' + d.slice(7);
    } else if (d.length > 6) {
        texto = '(' + d.slice(0, 2) + ') ' + d.slice(2, 6) + '-' + d.slice(6);
    } else if (d.length > 2) {
        texto = '(' + d.slice(0, 2) + ') ' + d.slice(2);
    } else if (d.length > 0) {
        texto = '(' + d;
    }

    campoTel.value = texto;
});


form.addEventListener('submit', function (e) {
    e.preventDefault();

    const dados = new FormData(form);
    const telefone = dados.get('telefone');

    if (telefone.replace(/\D/g, '').length < 10) {
        avisar('Digite um telefone com DDD.', 'erro');
        campoTel.focus();
        return;
    }

    const data = lerData(dados.get('data'));
    const dataTexto = doisDigitos(data.getDate()) + '/' +
        doisDigitos(data.getMonth() + 1) + '/' + data.getFullYear() +
        ' (' + DIAS[data.getDay()] + ')';

    let texto = 'Olá, Villa Serena! Gostaria de reservar uma mesa.\n\n' +
        'Nome: ' + dados.get('nome') + '\n' +
        'Telefone: ' + telefone + '\n' +
        'Data: ' + dataTexto + '\n' +
        'Horário: ' + dados.get('hora') + '\n' +
        'Pessoas: ' + dados.get('pessoas');

    if (dados.get('ocasiao') !== 'Nenhuma') {
        texto += '\nOcasião: ' + dados.get('ocasiao');
    }
    if (dados.get('obs').trim()) {
        texto += '\nObservações: ' + dados.get('obs').trim();
    }

    const url = 'https://wa.me/' + CONFIG.whatsapp + '?text=' + encodeURIComponent(texto);
    window.open(url, '_blank', 'noopener');

    avisar('Quase lá! Termine o envio no WhatsApp. A reserva só é confirmada quando o restaurante responder.', 'ok');
});

const headerTransparente = document.querySelector('.header-transparente');

if (headerTransparente) {
    const atualizarHeader = function () {
        headerTransparente.classList.toggle('rolou', window.scrollY > 40);
    };

    window.addEventListener('scroll', atualizarHeader, { passive: true });
    atualizarHeader();
}