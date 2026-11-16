const header = document.querySelector('.header');
const btnMenu = document.getElementById('btnMenu');

btnMenu.addEventListener('click', function () {
    const aberto = header.classList.toggle('aberto');
    btnMenu.setAttribute('aria-expanded', aberto);
});


const abas = document.querySelectorAll('.aba');
const forms = {
    entrar: document.getElementById('formEntrar'),
    cadastro: document.getElementById('formCadastro')
};
const aviso = document.getElementById('aviso');

function avisar(texto, tipo) {
    aviso.textContent = texto;
    aviso.className = 'aviso ' + tipo;
}

function limparAviso() {
    aviso.textContent = '';
    aviso.className = 'aviso';
}

function mostrarAba(nome) {
    if (!forms[nome]) nome = 'entrar';
    Object.keys(forms).forEach(function (chave) {
        forms[chave].hidden = (chave !== nome);
    });
    abas.forEach(function (aba) {
        aba.classList.toggle('ativa', aba.dataset.aba === nome);
    });
    limparAviso();
}

function irPara(nome) {
    history.replaceState(null, '', '#' + nome);
    mostrarAba(nome);
}

abas.forEach(function (aba) {
    aba.addEventListener('click', function () { irPara(aba.dataset.aba); });
});

document.querySelectorAll('[data-ir]').forEach(function (botao) {
    botao.addEventListener('click', function () { irPara(botao.dataset.ir); });
});


window.addEventListener('hashchange', function () {
    mostrarAba(location.hash.slice(1));
});

mostrarAba(location.hash.slice(1));


function lerUsuarios() {
    try {
        return JSON.parse(localStorage.getItem('usuarios')) || [];
    } catch (e) {
        return [];
    }
}

function salvarUsuarios(lista) {
    localStorage.setItem('usuarios', JSON.stringify(lista));
}


async function hashSenha(email, senha) {
    const dados = new TextEncoder().encode(email + ':' + senha);
    const buffer = await crypto.subtle.digest('SHA-256', dados);
    return Array.from(new Uint8Array(buffer))
        .map(function (b) { return b.toString(16).padStart(2, '0'); })
        .join('');
}

function navegadorSuportaHash() {
    return window.crypto && window.crypto.subtle;
}

function irParaInicio() {
    setTimeout(function () {
        location.href = '../Entrada/index.html';
    }, 1200);
}


forms.cadastro.addEventListener('submit', async function (e) {
    e.preventDefault();

    if (!navegadorSuportaHash()) {
        avisar('Abra o site pelo Live Server (localhost) para criar contas.', 'erro');
        return;
    }

    const nome = document.getElementById('cad-nome').value.trim();
    const email = document.getElementById('cad-email').value.trim().toLowerCase();
    const senha = document.getElementById('cad-senha').value;
    const confirma = document.getElementById('cad-confirma').value;

    if (senha.length < 6) {
        avisar('A senha precisa ter pelo menos 6 caracteres.', 'erro');
        return;
    }
    if (senha !== confirma) {
        avisar('As senhas não são iguais.', 'erro');
        return;
    }

    const usuarios = lerUsuarios();
    if (usuarios.some(function (u) { return u.email === email; })) {
        avisar('Já existe uma conta com esse e-mail. Tente entrar.', 'erro');
        return;
    }

    usuarios.push({ nome: nome, email: email, senha: await hashSenha(email, senha) });
    salvarUsuarios(usuarios);
    localStorage.setItem('usuarioLogado', JSON.stringify({ nome: nome, email: email }));

    avisar('Conta criada! Redirecionando...', 'ok');
    irParaInicio();
});


forms.entrar.addEventListener('submit', async function (e) {
    e.preventDefault();

    if (!navegadorSuportaHash()) {
        avisar('Abra o site pelo Live Server (localhost) para entrar.', 'erro');
        return;
    }

    const email = document.getElementById('login-email').value.trim().toLowerCase();
    const senha = document.getElementById('login-senha').value;

    const usuario = lerUsuarios().find(function (u) { return u.email === email; });
    const hash = await hashSenha(email, senha);

    if (!usuario || usuario.senha !== hash) {
        avisar('E-mail ou senha incorretos.', 'erro');
        return;
    }

    localStorage.setItem('usuarioLogado', JSON.stringify({ nome: usuario.nome, email: email }));
    avisar('Bem-vindo, ' + usuario.nome + '! Redirecionando...', 'ok');
    irParaInicio();
});


document.querySelectorAll('.btn-social').forEach(function (botao) {
    botao.addEventListener('click', function () {
        const provedor = botao.dataset.provedor;

        try {
            localStorage.setItem('usuarioLogado', JSON.stringify({ provedor: provedor, demo: true }));
        } catch (e) {}

        avisar('Modo demonstração: você entrou como visitante (' + provedor + '). Redirecionando...', 'ok');
        irParaInicio();
    });
});

const headerTransparente = document.querySelector('.header-transparente');

if (headerTransparente) {
    const atualizarHeader = function () {
        headerTransparente.classList.toggle('rolou', window.scrollY > 40);
    };

    window.addEventListener('scroll', atualizarHeader, { passive: true });
    atualizarHeader();
}

