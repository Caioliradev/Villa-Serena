document.querySelectorAll('.btn-tema').forEach(function (botao) {
    botao.addEventListener('click', function () {
        var tema = botao.dataset.tema;
        document.documentElement.setAttribute('data-tema', tema);
        try {
            localStorage.setItem('tema', tema);
        } catch (e) {}
    });
});

const header = document.querySelector('.header');
const btnMenu = document.getElementById('btnMenu');

btnMenu.addEventListener('click', function () {
    const aberto = header.classList.toggle('aberto');
    btnMenu.setAttribute('aria-expanded', aberto);
});


document.querySelectorAll('.menu a').forEach(function (link) {
    link.addEventListener('click', function () {
        header.classList.remove('aberto');
        btnMenu.setAttribute('aria-expanded', 'false');
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

