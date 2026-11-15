const header = document.querySelector('.header');
const btnMenu = document.getElementById('btnMenu');

btnMenu.addEventListener('click', function () {
    const aberto = header.classList.toggle('aberto');
    btnMenu.setAttribute('aria-expanded', aberto);
});

const lista = document.getElementById('listaItens');
const resumo = document.getElementById('resumo');
const resumoQtd = document.getElementById('resumoQtd');
const resumoTotal = document.getElementById('resumoTotal');
const campoObs = document.getElementById('obs');
const campoPagamento = document.getElementById('pagamento');
const btnFinalizar = document.getElementById('btnFinalizar');
const aviso = document.getElementById('aviso');

function escapar(texto) {
    return String(texto).replace(/[&<>"']/g, function (c) {
        return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c];
    });
}

function avisar(html, tipo) {
    aviso.innerHTML = html;
    aviso.className = 'aviso ' + tipo;
}

function desenhar() {
    const itens = Loja.carrinho();

    if (itens.length === 0) {
        resumo.hidden = true;
        lista.innerHTML =
            '<div class="vazio">' +
                '<h2>Seu carrinho está vazio</h2>' +
                '<p>Escolha seus pratos no cardápio e eles aparecem aqui.</p>' +
                '<a href="../Cardapio/cardapio.html" class="btn-pilula">Ver cardápio <span class="seta">↗</span></a>' +
            '</div>';
        return;
    }

    resumo.hidden = false;

    lista.innerHTML = itens.map(function (i) {
        return `
            <article class="item" data-id="${escapar(i.id)}">
                <div class="item-img">
                    <img src="../Cardapio/${escapar(i.imagem)}" alt="" onerror="this.remove()">
                </div>
                <div class="item-info">
                    <h3>${escapar(i.nome)}</h3>
                    <span class="item-preco">${Loja.formatarPreco(i.preco)}</span>
                </div>
                <div class="item-qtd">
                    <button type="button" data-acao="menos" aria-label="Diminuir">−</button>
                    <span>${i.qtd}</span>
                    <button type="button" data-acao="mais" aria-label="Aumentar">+</button>
                </div>
                <span class="item-subtotal">${Loja.formatarPreco(i.preco * i.qtd)}</span>
                <button type="button" class="item-remover" data-acao="remover" aria-label="Remover ${escapar(i.nome)}">✕</button>
            </article>
        `;
    }).join('');

    resumoQtd.textContent = Loja.quantidadeTotal();
    resumoTotal.textContent = Loja.formatarPreco(Loja.total(itens));
}

lista.addEventListener('click', function (e) {
    const botao = e.target.closest('[data-acao]');
    if (!botao) return;

    const id = botao.closest('.item').dataset.id;
    let itens = Loja.carrinho();
    const item = itens.find(function (i) { return i.id === id; });
    if (!item) return;

    if (botao.dataset.acao === 'mais') item.qtd += 1;
    if (botao.dataset.acao === 'menos') item.qtd -= 1;

    if (botao.dataset.acao === 'remover' || item.qtd <= 0) {
        itens = itens.filter(function (i) { return i.id !== id; });
    }

    Loja.guardarCarrinho(itens);
    avisar('', '');
    desenhar();
});

function enviarParaSistema() {
    fetch('http://localhost:5000/pedido/', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(Loja.paraJsonDoSistema())
    }).catch(function () {
        console.warn('App Delivery fechado: o pedido será enviado no próximo pedido.');
    });
}

btnFinalizar.addEventListener('click', function () {
    const itens = Loja.carrinho();
    if (itens.length === 0) return;

    const usuario = Loja.usuario();
    if (!usuario) {
        avisar('Entre na sua conta para finalizar. <a href="../Cadastro/cadastro.html#entrar">Entrar</a>', 'erro');
        return;
    }

    if (!campoPagamento.value) {
        avisar('Escolha a forma de pagamento.', 'erro');
        return;
    }

    const perfil = Loja.ler('perfil:' + Loja.chaveUsuario(usuario), {});
    const nomeEntrega = perfil.nome || usuario.nome || '';
    if (!nomeEntrega || !perfil.telefone || !perfil.endereco) {
        avisar('Preencha nome, telefone e endereço no <a href="../Perfil/perfil.html">seu perfil</a> para finalizar.', 'erro');
        return;
    }

    const pedido = {
        id: String(Date.now()).slice(-6),
        data: new Date().toISOString(),
        itens: itens,
        total: Loja.total(itens),
        obs: campoObs.value.trim(),
        status: 'Recebido',
        cliente: Loja.chaveUsuario(usuario),
        pagamento: campoPagamento.value,
        entrega: { nome: nomeEntrega, telefone: perfil.telefone, endereco: perfil.endereco }
    };

    const pedidos = Loja.ler('pedidos', []);
    pedidos.unshift(pedido);
    Loja.salvar('pedidos', pedidos);
    Loja.guardarCarrinho([]);

    enviarParaSistema();

    resumo.hidden = true;
    lista.innerHTML =
        '<div class="vazio">' +
            '<h2>Pedido #' + pedido.id + ' recebido!</h2>' +
            '<p>Projeto de demonstração: o pedido fica salvo só neste navegador.</p>' +
            '<a href="../Carrinho/carrinho.html" class="btn-pilula">Ver meus pedidos <span class="seta">↗</span></a>' +
        '</div>';
});

desenhar();