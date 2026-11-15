const pratos = [
    // ENTRADAS
    { categoria: 'entradas', nome: 'Bruschetta de tomate', descricao: 'Pão italiano tostado, tomate fresco, manjericão e azeite.', preco: 28, imagem: 'image/comidas/Bruschetta-de-tomate.png' },
    { categoria: 'entradas', nome: 'Carpaccio de carne', descricao: 'Lâminas finas de filé, rúcula, parmesão e molho de mostarda.', preco: 46, imagem: 'image/comidas/Carpaccio-de-carne.png' },
    { categoria: 'entradas', nome: 'Salada Serena', descricao: 'Folhas verdes, tomate cereja, castanhas e vinagrete de mel.', preco: 34, imagem: 'image/comidas/Salada-Serena.png' },

    // PRATOS PRINCIPAIS
    { categoria: 'principais', nome: 'Risoto de cogumelos', descricao: 'Arroz arbóreo cremoso com cogumelos frescos e parmesão.', preco: 68, imagem: 'image/comidas/Risoto-de-cogumelos.png' },
    { categoria: 'principais', nome: 'Salmão grelhado', descricao: 'Salmão com legumes salteados e molho de limão siciliano.', preco: 82, imagem: 'image/comidas/Salmão-grelhado.png' },
    { categoria: 'principais', nome: 'Legumes flambados da casa', descricao: 'Mix de legumes salteados e flambados na frigideira.', preco: 58, imagem: 'image/comidas/Legumes-flambados-da-casa.png' },

    // SOBREMESAS
    { categoria: 'sobremesas', nome: 'Tiramisù', descricao: 'Clássico italiano com café, mascarpone e cacau.', preco: 26, imagem: 'image/comidas/Tiramisù.png' },
    { categoria: 'sobremesas', nome: 'Petit gâteau', descricao: 'Bolinho de chocolate quente com sorvete de creme.', preco: 30, imagem: 'image/comidas/Petit-gâteau.png' },
    { categoria: 'sobremesas', nome: 'Panna cotta', descricao: 'Creme de baunilha com calda de frutas vermelhas.', preco: 24, imagem: 'image/comidas/Panna-cotta.png' },

    // BEBIDAS
    { categoria: 'bebidas', nome: 'Suco natural', descricao: 'Laranja, limão, maracujá ou abacaxi com hortelã.', preco: 14, imagem: 'image/comidas/Suco-natural.png' },
    { categoria: 'bebidas', nome: 'Limonada italiana', descricao: 'Limão siciliano, água com gás e xarope de baunilha.', preco: 16, imagem: 'image/comidas/Limonada-italiana.png' },
    { categoria: 'bebidas', nome: 'Vinho da casa (taça)', descricao: 'Seleção de tinto ou branco, perguntar ao garçom.', preco: 32, imagem: 'image/comidas/Vinho-da-casa-(taça).png' }
];

pratos.forEach(function (p) {
    p.id = normalizar(p.nome).replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '');
});

const grade = document.getElementById('gradePratos');
const filtros = document.querySelectorAll('.filtro');
const campoBusca = document.getElementById('busca');
const campoOrdem = document.getElementById('ordenar');
const infoResultado = document.getElementById('infoResultado');

let categoriaAtual = 'todos';

function formatarPreco(valor) {
    return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function normalizar(texto) {
    return texto.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
}

function desenharPratos(lista) {
    if (lista.length === 0) {
        grade.innerHTML = '<p class="sem-resultado" id="semResultado"></p>';
        const termo = campoBusca.value.trim();
        document.getElementById('semResultado').textContent =
            termo ? 'Nenhum prato encontrado para "' + termo + '".' : 'Nenhum prato nesta categoria.';
        return;
    }

    grade.innerHTML = lista.map(function (p) {
        return `
            <article class="prato">
                <div class="prato-img">
                    <img src="${p.imagem}" alt="${p.nome}" loading="lazy" onerror="this.remove()">
                </div>
                <div class="prato-info">
                    <div class="prato-linha">
                        <h3>${p.nome}</h3>
                        <span class="prato-preco">${formatarPreco(p.preco)}</span>
                    </div>
                    <p>${p.descricao}</p>
                    <button type="button" class="btn-adicionar" data-id="${p.id}">Adicionar ao carrinho</button>
                </div>
            </article>
        `;
    }).join('');
}

function atualizar() {
    const termo = normalizar(campoBusca.value.trim());

    let lista = pratos.filter(function (p) {
        return categoriaAtual === 'todos' || p.categoria === categoriaAtual;
    });

    if (termo) {
        lista = lista.filter(function (p) {
            return normalizar(p.nome + ' ' + p.descricao).includes(termo);
        });
    }

    if (campoOrdem.value === 'menor') {
        lista = lista.slice().sort(function (a, b) { return a.preco - b.preco; });
    } else if (campoOrdem.value === 'maior') {
        lista = lista.slice().sort(function (a, b) { return b.preco - a.preco; });
    } else if (campoOrdem.value === 'az') {
        lista = lista.slice().sort(function (a, b) { return a.nome.localeCompare(b.nome, 'pt-BR'); });
    }

    desenharPratos(lista);

    infoResultado.textContent = lista.length === 1 ? '1 prato' : lista.length + ' pratos';
}

filtros.forEach(function (botao) {
    botao.addEventListener('click', function () {
        filtros.forEach(function (b) { b.classList.remove('ativo'); });
        botao.classList.add('ativo');
        categoriaAtual = botao.dataset.categoria;
        atualizar();
    });
});

campoBusca.addEventListener('input', atualizar);
campoOrdem.addEventListener('change', atualizar);

grade.addEventListener('click', function (e) {
    const botao = e.target.closest('.btn-adicionar');
    if (!botao) return;

    const prato = pratos.find(function (p) { return p.id === botao.dataset.id; });
    Loja.adicionar(prato);

    botao.textContent = 'Adicionado ✓';
    botao.classList.add('adicionado');
    setTimeout(function () {
        botao.textContent = 'Adicionar ao carrinho';
        botao.classList.remove('adicionado');
    }, 1200);
});

atualizar();

const header = document.querySelector('.header');
const btnMenu = document.getElementById('btnMenu');

btnMenu.addEventListener('click', function () {
    const aberto = header.classList.toggle('aberto');
    btnMenu.setAttribute('aria-expanded', aberto);
});