const CONFIG = {
    whatsapp: '5511999999999',          
    endereco: 'Rua Exemplo, 123, Centro' 
};

const header = document.querySelector('.header');
const btnMenu = document.getElementById('btnMenu');

btnMenu.addEventListener('click', function () {
    const aberto = header.classList.toggle('aberto');
    btnMenu.setAttribute('aria-expanded', aberto);
});


document.getElementById('mapa').src =
    'https://www.google.com/maps?q=' + encodeURIComponent(CONFIG.endereco) + '&output=embed';


document.querySelectorAll('[data-tel]').forEach(function (link) {
    link.href = 'tel:+' + CONFIG.whatsapp;
});


const form = document.getElementById('formContato');

form.addEventListener('submit', function (e) {
    e.preventDefault();

    const dados = new FormData(form);
    const texto =
        'Olá, Villa Serena! Meu nome é ' + dados.get('nome') + '.\n' +
        'Assunto: ' + dados.get('assunto') + '\n' +
        'E-mail: ' + dados.get('email') + '\n\n' +
        dados.get('mensagem');

    const url = 'https://wa.me/' + CONFIG.whatsapp + '?text=' + encodeURIComponent(texto);
    window.open(url, '_blank', 'noopener');
    form.reset();
});


const blocos = document.querySelectorAll('.info-card, .contato-form, .contato-mapa');

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