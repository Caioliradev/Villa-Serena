const header = document.querySelector('.header');
const btnMenu = document.getElementById('btnMenu');

btnMenu.addEventListener('click', function () {
    const aberto = header.classList.toggle('aberto');
    btnMenu.setAttribute('aria-expanded', aberto);
});

const semLogin = document.getElementById('semLogin');
const painel = document.getElementById('painelPerfil');
const form = document.getElementById('formPerfil');
const campoNome = document.getElementById('p-nome');
const campoEmail = document.getElementById('p-email');
const campoTel = document.getElementById('p-tel');
const campoEnd = document.getElementById('p-end');
const aviso = document.getElementById('aviso');

const usuario = Loja.usuario();

function mostrarNome(nome) {
    const exibido = nome || 'Visitante';
    document.getElementById('perfilNome').textContent = exibido;
    document.getElementById('avatarGrande').textContent = exibido.charAt(0).toUpperCase();
}

function carregar() {
    const chave = Loja.chaveUsuario(usuario);
    const dados = Loja.ler('perfil:' + chave, {});
    const nome = dados.nome || usuario.nome || '';

    campoNome.value = nome;
    campoTel.value = dados.telefone || '';
    campoEnd.value = dados.endereco || '';

    const textoEmail = usuario.email || ('Entrou com ' + usuario.provedor + ' (demonstração)');
    campoEmail.value = textoEmail;
    document.getElementById('perfilEmail').textContent = textoEmail;

    mostrarNome(nome);

    const meusPedidos = Loja.ler('pedidos', []).filter(function (p) { return p.cliente === chave; });
    document.getElementById('qtdPedidos').textContent = meusPedidos.length;
}

if (!usuario) {
    semLogin.hidden = false;
} else {
    painel.hidden = false;
    carregar();
}

form.addEventListener('submit', function (e) {
    e.preventDefault();

    const nome = campoNome.value.trim();
    const chave = Loja.chaveUsuario(usuario);

    Loja.salvar('perfil:' + chave, {
        nome: nome,
        telefone: campoTel.value.trim(),
        endereco: campoEnd.value.trim()
    });

    usuario.nome = nome;
    Loja.salvar('usuarioLogado', usuario);

    // Se for uma conta criada no cadastro, atualiza o nome lá também
    const contas = Loja.ler('usuarios', []);
    const conta = contas.find(function (c) { return c.email === usuario.email; });
    if (conta) {
        conta.nome = nome;
        Loja.salvar('usuarios', contas);
    }

    mostrarNome(nome);
    Loja.atualizarCabecalho();
    aviso.textContent = 'Dados salvos!';
    aviso.className = 'aviso ok';
});

document.getElementById('btnExportar').addEventListener('click', function (e) {
    e.preventDefault();
    const qtd = Loja.exportarPedidos();
    aviso.textContent = qtd
        ? qtd + ' pedido(s) exportado(s) em pedidos.json.'
        : 'Nenhum pedido completo para exportar. Faça um pedido com pagamento e endereço preenchidos.';
    aviso.className = qtd ? 'aviso ok' : 'aviso erro';
});

document.getElementById('btnSair').addEventListener('click', function () {
    try {
        localStorage.removeItem('usuarioLogado');
    } catch (e) {}
    location.href = '../Entrada/index.html';
});