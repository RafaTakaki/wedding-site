const targetDate = new Date('2027-06-26T00:00:00-03:00');

function updateCountdown() {
  const now = new Date();
  const diff = targetDate - now;

  if (diff <= 0) {
    document.getElementById('days').textContent = '00';
    document.getElementById('hours').textContent = '00';
    document.getElementById('minutes').textContent = '00';
    document.getElementById('seconds').textContent = '00';
    return;
  }

  const days = Math.floor(diff / (1000 * 60 * 60 * 24));
  const hours = Math.floor((diff / (1000 * 60 * 60)) % 24);
  const minutes = Math.floor((diff / (1000 * 60)) % 60);
  const seconds = Math.floor((diff / 1000) % 60);

  document.getElementById('days').textContent = String(days).padStart(2, '0');
  document.getElementById('hours').textContent = String(hours).padStart(2, '0');
  document.getElementById('minutes').textContent = String(minutes).padStart(2, '0');
  document.getElementById('seconds').textContent = String(seconds).padStart(2, '0');
}

updateCountdown();
setInterval(updateCountdown, 1000);

const copyButton = document.getElementById('copyPix');
const pixValue = document.getElementById('pixValue');

if (copyButton && pixValue) {
  copyButton.addEventListener('click', async () => {
    const text = pixValue.textContent.trim();

    try {
      await navigator.clipboard.writeText(text);
      copyButton.textContent = 'Copiado';
      setTimeout(() => {
        copyButton.textContent = 'Copiar chave';
      }, 1400);
    } catch (error) {
      copyButton.textContent = 'Não disponível';
      setTimeout(() => {
        copyButton.textContent = 'Copiar chave';
      }, 1400);
    }
  });
}

document.querySelectorAll('.faq-question').forEach((button) => {
  button.addEventListener('click', () => {
    const item = button.closest('.faq-item');
    const expanded = button.getAttribute('aria-expanded') === 'true';

    document.querySelectorAll('.faq-item').forEach((faq) => {
      faq.classList.remove('active');
      faq.querySelector('.faq-question').setAttribute('aria-expanded', 'false');
    });

    if (!expanded) {
      item.classList.add('active');
      button.setAttribute('aria-expanded', 'true');
    }
  });
});
