const header = document.querySelector('.header');
const btnMenu = document.getElementById('btnMenu');

btnMenu.addEventListener('click', function () {
    const aberto = header.classList.toggle('aberto');
    btnMenu.setAttribute('aria-expanded', aberto);
});


const blocos = document.querySelectorAll('.sobre-historia, .stat, .valor, .sobre-cta');

const observador = new IntersectionObserver(function (entradas) {
    entradas.forEach(function (entrada) {
        if (entrada.isIntersecting) {
            entrada.target.classList.add('visivel');
            observador.unobserve(entrada.target);
        }
    });
}, { threshold: 0.15 });

blocos.forEach(function (bloco) {
    bloco.classList.add('revelar');
    observador.observe(bloco);
});

const headerTransparente = document.querySelector('.header-transparente');

if (headerTransparente) {
    const atualizarHeader = function () {
        headerTransparente.classList.toggle('rolou', window.scrollY > 40);
    };

    window.addEventListener('scroll', atualizarHeader, { passive: true });
    atualizarHeader();
}