(function () {

    function ler(chave, padrao) {
        try {
            const valor = JSON.parse(localStorage.getItem(chave));
            return valor === null ? padrao : valor;
        } catch (e) {
            return padrao;
        }
    }

    function salvar(chave, valor) {
        try {
            localStorage.setItem(chave, JSON.stringify(valor));
        } catch (e) {}
    }

    const Loja = {
        ler: ler,
        salvar: salvar,

        formatarPreco: function (valor) {
            return valor.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
        },

        carrinho: function () {
            const itens = ler('carrinho', []);
            return Array.isArray(itens) ? itens : [];
        },
        guardarCarrinho: function (itens) {
            salvar('carrinho', itens);
            Loja.atualizarCabecalho();
        },
        adicionar: function (prato) {
            const itens = Loja.carrinho();
            const existente = itens.find(function (i) { return i.id === prato.id; });
            if (existente) {
                existente.qtd += 1;
            } else {
                itens.push({ id: prato.id, nome: prato.nome, preco: prato.preco, imagem: prato.imagem, qtd: 1 });
            }
            Loja.guardarCarrinho(itens);
        },
        quantidadeTotal: function () {
            return Loja.carrinho().reduce(function (soma, i) { return soma + i.qtd; }, 0);
        },
        total: function (itens) {
            return itens.reduce(function (soma, i) { return soma + i.preco * i.qtd; }, 0);
        },

        usuario: function () {
            return ler('usuarioLogado', null);
        },
        chaveUsuario: function (u) {
            return u.email || ('visitante-' + String(u.provedor || 'demo').toLowerCase());
        },

                paraJsonDoSistema: function () {
            const dois = function (n) { return String(n).padStart(2, '0'); };
            const limpar = function (id) { return String(id).replace(/^-+|-+$/g, ''); };

            const pedidos = ler('pedidos', [])
                .filter(function (p) { return p.entrega && p.pagamento; })
                .reverse()
                .map(function (p) {
                    const d = new Date(p.data);
                    const dia = d.getFullYear() + dois(d.getMonth() + 1) + dois(d.getDate());
                    return {
                        id: 'PED-' + dia + '-' + p.id,
                        data: d.getFullYear() + '-' + dois(d.getMonth() + 1) + '-' + dois(d.getDate()) +
                              'T' + dois(d.getHours()) + ':' + dois(d.getMinutes()) + ':' + dois(d.getSeconds()),
                        cliente: {
                            nome: p.entrega.nome,
                            telefone: p.entrega.telefone,
                            endereco: p.entrega.endereco
                        },
                        pagamento: p.pagamento,
                        itens: p.itens.map(function (i) {
                            return { codigo: limpar(i.id), quantidade: i.qtd, preco: i.preco };
                        }),
                        total: p.total
                    };
                });

            return { pedidos: pedidos };
        },

        exportarPedidos: function () {
            const dados = Loja.paraJsonDoSistema();
            if (dados.pedidos.length === 0) return 0;

            const blob = new Blob([JSON.stringify(dados, null, 2)], { type: 'application/json' });
            const link = document.createElement('a');
            link.href = URL.createObjectURL(blob);
            link.download = 'pedidos.json';
            document.body.appendChild(link);
            link.click();
            link.remove();
            setTimeout(function () { URL.revokeObjectURL(link.href); }, 1000);
            return dados.pedidos.length;
        },

        atualizarCabecalho: function () {
            const total = Loja.quantidadeTotal();
            document.querySelectorAll('.carrinho-contador').forEach(function (el) {
                el.textContent = total;
                el.hidden = total === 0;
            });

            const u = Loja.usuario();
            document.querySelectorAll('[data-visitante]').forEach(function (el) {
                el.style.display = u ? 'none' : '';
            });

            const link = document.getElementById('linkPerfil');
            if (link) {
                link.style.display = u ? 'flex' : 'none';
                if (u) {
                    const nome = u.nome || 'Visitante';
                    document.getElementById('nomePerfil').textContent = 'Olá, ' + nome.split(' ')[0];
                    document.getElementById('avatarMini').textContent = nome.charAt(0).toUpperCase();
                }
            }
        }
    };

    window.Loja = Loja;

    const cabecalho = document.querySelector('.header-transparente');
    if (cabecalho) {
        const marcarRolagem = function () {
            cabecalho.classList.toggle('rolou', window.scrollY > 40);
        };
        window.addEventListener('scroll', marcarRolagem, { passive: true });
        marcarRolagem();
    }

    Loja.atualizarCabecalho();
    window.addEventListener('storage', Loja.atualizarCabecalho);
    window.addEventListener('pageshow', Loja.atualizarCabecalho);
})();