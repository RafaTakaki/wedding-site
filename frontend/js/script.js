document.addEventListener('DOMContentLoaded', () => {
  const image = document.querySelector('.foto');

  if (image) {
    image.addEventListener('error', () => {
      image.style.display = 'none';
    });
  }

  const page = document.body.dataset.page;

  if (page === 'home') {
    initHomePage();
  }

  if (page === 'confirmacao') {
    initConfirmacaoPage();
  }

  if (page === 'presentes') {
    initPresentesPage();
  }

  if (page === 'admin') {
    initAdminPage();
  }
});

function apiBaseUrl() {
  return (document.body.dataset.apiBase || 'http://localhost:5000').replace(/\/$/, '');
}

async function readApiError(response) {
  try {
    const body = await response.json();
    return body.message || body.detail || 'Não foi possível concluir a operação.';
  } catch (error) {
    return 'Não foi possível conectar ao servidor. Confira se a API está em execução.';
  }
}

function initHomePage() {
  const targetDate = new Date('2027-06-26T18:00:00-03:00');
  const countdown = document.querySelector('[data-countdown]');

  if (!countdown) return;

  const updateCountdown = () => {
    const diff = targetDate.getTime() - Date.now();

    if (diff <= 0) {
      countdown.innerHTML = '<div class="countdown-item"><strong>Hoje</strong><span>É o dia!</span></div>';
      return;
    }

    const totalSeconds = Math.floor(diff / 1000);
    const days = Math.floor(totalSeconds / 86400);
    const hours = Math.floor((totalSeconds % 86400) / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const seconds = totalSeconds % 60;

    countdown.innerHTML = `
      <div class="countdown-item">
        <strong>${days}</strong>
        <span>dias</span>
      </div>
      <div class="countdown-item">
        <strong>${hours}</strong>
        <span>horas</span>
      </div>
      <div class="countdown-item">
        <strong>${minutes}</strong>
        <span>minutos</span>
      </div>
      <div class="countdown-item">
        <strong>${seconds}</strong>
        <span>segundos</span>
      </div>
    `;
  };

  updateCountdown();
  setInterval(updateCountdown, 1000);
}

function initPresentesPage() {
  const grid = document.getElementById('giftGrid');
  const message = document.getElementById('giftMessage');
  const dialog = document.getElementById('giftDialog');
  const dialogDescription = document.getElementById('giftDialogDescription');
  const guestNameInput = document.getElementById('giftGuestName');
  const claimForm = document.getElementById('giftClaimForm');
  let selectedGift = null;

  if (!grid || !message || !dialog || !claimForm) return;

  const formatAmount = (amountInCents) => new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL'
  }).format(amountInCents / 100);

  const loadGifts = async () => {
    message.textContent = 'Carregando presentes...';
    try {
      const response = await fetch(`${apiBaseUrl()}/api/gifts`);
      if (!response.ok) throw new Error(await readApiError(response));
      const gifts = await response.json();
      grid.replaceChildren();

      if (gifts.length === 0) {
        message.textContent = 'A lista de presentes será publicada em breve.';
        return;
      }

      message.textContent = 'Presentes simbólicos; a escolha registra uma intenção e não processa pagamento.';
      gifts.forEach((gift) => {
        const card = document.createElement('article');
        card.className = 'gift-card';
        const illustration = document.createElement('div');
        illustration.className = 'gift-illustration';
        if (gift.imageUrl) {
          const image = document.createElement('img');
          image.src = gift.imageUrl;
          image.alt = gift.name;
          image.loading = 'lazy';
          image.addEventListener('error', () => {
            image.remove();
            illustration.textContent = gift.name;
          });
          illustration.append(image);
        } else {
          illustration.textContent = gift.name;
        }

        const details = document.createElement('div');
        const title = document.createElement('h2');
        title.textContent = gift.name;
        const description = document.createElement('p');
        description.textContent = gift.description;
        const amount = document.createElement('p');
        amount.textContent = formatAmount(gift.amountInCents);
        details.append(title, description, amount);

        const meta = document.createElement('div');
        meta.className = 'gift-meta';
        const status = document.createElement('span');
        status.className = 'gift-status';
        const claimButton = document.createElement('button');
        claimButton.className = 'reserve-btn';
        claimButton.type = 'button';

        if (gift.status === 'available') {
          status.textContent = 'Disponível';
          claimButton.textContent = 'Escolher';
          claimButton.addEventListener('click', () => {
            selectedGift = gift;
            dialogDescription.textContent = `${gift.name} · ${formatAmount(gift.amountInCents)}`;
            guestNameInput.value = '';
            dialog.showModal();
            guestNameInput.focus();
          });
        } else {
          status.textContent = 'Escolhido';
          status.classList.add('reserved');
          claimButton.textContent = 'Indisponível';
          claimButton.disabled = true;
        }

        meta.append(status, claimButton);
        card.append(illustration, details, meta);
        grid.append(card);
      });
    } catch (error) {
      grid.replaceChildren();
      message.textContent = error.message;
    }
  };

  dialog.querySelector('[data-cancel-gift]').addEventListener('click', () => dialog.close());
  dialog.addEventListener('click', (event) => {
    if (event.target === dialog) dialog.close();
  });

  claimForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    if (!selectedGift) return;
    const submitButton = claimForm.querySelector('[type="submit"]');
    submitButton.disabled = true;
    try {
      const response = await fetch(`${apiBaseUrl()}/api/gifts/${selectedGift.id}/claim`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ guestName: guestNameInput.value.trim() })
      });
      if (!response.ok) throw new Error(await readApiError(response));
      dialog.close();
      selectedGift = null;
      message.textContent = 'Sua escolha foi registrada. O pagamento em dinheiro será combinado com os noivos; nenhuma cobrança foi feita neste site.';
      await loadGifts();
      message.textContent = 'Sua escolha foi registrada. O pagamento em dinheiro será combinado com os noivos; nenhuma cobrança foi feita neste site.';
    } catch (error) {
      message.textContent = error.message;
    } finally {
      submitButton.disabled = false;
    }
  });

  loadGifts();
}

function initConfirmacaoPage() {
  const input = document.getElementById('nome');
  const buscar = document.getElementById('buscarConvite');
  const resultado = document.getElementById('resultado');
  const lista = document.getElementById('listaConvidados');
  const companionArea = document.getElementById('acompanhanteLivre');
  const salvar = document.getElementById('salvarConfirmacao');
  const mensagem = document.getElementById('mensagemStatus');

  if (!input || !buscar || !resultado || !lista || !salvar || !mensagem) {
    return;
  }

  let conviteAtual = null;

  const resetMensagem = () => {
    mensagem.textContent = '';
    mensagem.classList.remove('success', 'error');
  };

  const montarLista = (registro) => {
    lista.replaceChildren();
    companionArea.replaceChildren();
    registro.people.forEach((person) => {
      const card = document.createElement('div');
      card.className = 'pessoa';
      const name = document.createElement('div');
      name.className = 'nome-pessoa';
      name.textContent = person.name;
      const options = document.createElement('div');
      options.className = 'opcoes';
      options.setAttribute('role', 'radiogroup');
      options.setAttribute('aria-label', `Status de ${person.name}`);

      [
        ['vou', 'Vou'],
        ['nao-vou', 'Não vou'],
        ['a-confirmar', 'A confirmar']
      ].forEach(([value, labelText]) => {
        const label = document.createElement('label');
        const radio = document.createElement('input');
        radio.type = 'radio';
        radio.name = `status-${person.id}`;
        radio.value = value;
        radio.checked = person.status === value;
        radio.disabled = !registro.isRsvpOpen || person.status !== 'a-confirmar';
        label.append(radio, document.createTextNode(labelText));
        options.append(label);
      });

      card.append(name, options);
      lista.append(card);
    });

    if (registro.isRsvpOpen && registro.allowFreeCompanions && registro.freeCompanionSlots > 0) {
      const form = document.createElement('form');
      form.className = 'pessoa';
      const label = document.createElement('label');
      label.className = 'field';
      label.htmlFor = 'nomeAcompanhanteLivre';
      label.textContent = `Adicionar acompanhante (${registro.freeCompanionSlots} vaga(s) disponível(is))`;
      const nameInput = document.createElement('input');
      nameInput.id = 'nomeAcompanhanteLivre';
      nameInput.name = 'name';
      nameInput.maxLength = 120;
      nameInput.required = true;
      nameInput.placeholder = 'Nome completo';
      nameInput.autocomplete = 'name';
      const addButton = document.createElement('button');
      addButton.type = 'submit';
      addButton.textContent = 'Adicionar acompanhante';
      form.append(label, nameInput, addButton);
      form.addEventListener('submit', async (event) => {
        event.preventDefault();
        addButton.disabled = true;
        try {
          const response = await fetch(`${apiBaseUrl()}/api/guests/rsvp/${encodeURIComponent(registro.rsvpToken)}/companions`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name: nameInput.value.trim() })
          });
          if (!response.ok) throw new Error(await readApiError(response));
          conviteAtual = await response.json();
          montarLista(conviteAtual);
          mensagem.textContent = 'Acompanhante adicionado ao convite.';
          mensagem.classList.add('visible', 'success');
        } catch (error) {
          mensagem.textContent = error.message;
          mensagem.classList.add('visible', 'error');
          addButton.disabled = false;
        }
      });
      companionArea.append(form);
    }

    salvar.disabled = !registro.isRsvpOpen || !registro.people.some((person) => person.status === 'a-confirmar');
    if (!registro.isRsvpOpen) {
      mensagem.textContent = 'O prazo para confirmação terminou. Entre em contato com os noivos.';
      mensagem.classList.add('visible', 'error');
    }
  };

  const atualizarResultado = (registro) => {
    resultado.replaceChildren();
    resultado.classList.add('visible');
    const title = document.createElement('strong');
    title.textContent = `Convite encontrado: ${registro.name}`;
    const detail = document.createElement('small');
    detail.textContent = 'Confirme cada pessoa individualmente antes do prazo.';
    resultado.append(title, document.createElement('br'), detail);
  };

  buscar.addEventListener('click', async () => {
    resetMensagem();
    const valor = input.value.trim();
    lista.replaceChildren();
    conviteAtual = null;

    if (!valor) {
      resultado.classList.add('visible');
      resultado.textContent = 'Digite seu nome completo para continuar.';
      return;
    }

    buscar.disabled = true;
    try {
      const response = await fetch(`${apiBaseUrl()}/api/guests/lookup?name=${encodeURIComponent(valor)}`);
      if (!response.ok) throw new Error(await readApiError(response));
      conviteAtual = await response.json();
      atualizarResultado(conviteAtual);
      montarLista(conviteAtual);
    } catch (error) {
      resultado.classList.add('visible');
      resultado.textContent = error.message;
    } finally {
      buscar.disabled = false;
    }
  });

  salvar.addEventListener('click', async () => {
    if (!conviteAtual) return;
    const answers = conviteAtual.people
      .filter((person) => person.status === 'a-confirmar')
      .map((person) => ({
        inviteeId: person.id,
        status: lista.querySelector(`input[name="status-${person.id}"]:checked`)?.value || person.status
      }));

    if (!answers.some((answer) => answer.status !== 'a-confirmar')) {
      mensagem.textContent = 'Escolha Vou ou Não vou para pelo menos uma pessoa.';
      mensagem.classList.add('visible', 'error');
      return;
    }

    salvar.disabled = true;
    try {
      const response = await fetch(`${apiBaseUrl()}/api/guests/rsvp/${encodeURIComponent(conviteAtual.rsvpToken)}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ answers })
      });
      if (!response.ok) throw new Error(await readApiError(response));
      answers.forEach((answer) => {
        const person = conviteAtual.people.find((item) => item.id === answer.inviteeId);
        if (person) person.status = answer.status;
      });
      montarLista(conviteAtual);
      mensagem.textContent = 'Confirmação salva. Respostas enviadas não podem ser alteradas pelo site.';
      mensagem.classList.add('visible', 'success');
    } catch (error) {
      mensagem.textContent = error.message;
      mensagem.classList.add('visible', 'error');
      salvar.disabled = false;
    }
  });
}

function initAdminPage() {
  const connectForm = document.getElementById('adminConnectForm');
  const keyInput = document.getElementById('adminKey');
  const createForm = document.getElementById('guestCreateForm');
  const message = document.getElementById('adminMessage');
  const actionMessage = document.getElementById('guestActionMessage');
  const guestList = document.getElementById('guestList');
  const giftAdminSection = document.getElementById('giftAdminSection');
  const giftCreateForm = document.getElementById('giftCreateForm');
  const giftAdminList = document.getElementById('giftAdminList');
  let adminKey = '';

  const adminFetch = (path, options = {}) => fetch(`${apiBaseUrl()}${path}`, {
    ...options,
    headers: {
      ...options.headers,
      'X-Admin-Key': adminKey
    }
  });

  const loadGifts = async () => {
    const response = await adminFetch('/api/admin/gifts');
    if (!response.ok) throw new Error(await readApiError(response));
    const gifts = await response.json();
    giftAdminList.replaceChildren();

    gifts.forEach((gift) => {
      const row = document.createElement('article');
      row.className = 'gift-admin-row';
      const info = document.createElement('div');
      const title = document.createElement('strong');
      title.textContent = `${gift.name} · ${new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(gift.amountInCents / 100)}`;
      const detail = document.createElement('p');
      detail.textContent = gift.status === 'available'
        ? `Disponível · ${gift.description}`
        : `Escolhido por ${gift.claimedBy || 'convidado'} · ${gift.description}`;
      info.append(title, detail);
      row.append(info);

      if (gift.status !== 'available') {
        const releaseButton = document.createElement('button');
        releaseButton.type = 'button';
        releaseButton.textContent = 'Liberar item';
        releaseButton.addEventListener('click', async () => {
          try {
            const releaseResponse = await adminFetch(`/api/admin/gifts/${gift.id}/release`, { method: 'POST' });
            if (!releaseResponse.ok) throw new Error(await readApiError(releaseResponse));
            await loadGifts();
            actionMessage.textContent = 'Presente liberado para nova escolha.';
          } catch (error) {
            actionMessage.textContent = error.message;
          }
        });
        row.append(releaseButton);
      }

      giftAdminList.append(row);
    });

    if (gifts.length === 0) {
      const empty = document.createElement('p');
      empty.textContent = 'Nenhum presente cadastrado.';
      giftAdminList.append(empty);
    }
  };

  const loadGuests = async () => {
    const response = await adminFetch('/api/admin/guests');
    if (!response.ok) throw new Error(await readApiError(response));
    const invitations = await response.json();
    guestList.replaceChildren();

    const counts = { vou: 0, 'nao-vou': 0, 'a-confirmar': 0 };
    invitations.forEach((invitation) => {
      const row = document.createElement('article');
      row.className = 'guest-row';
      row.dataset.invitationId = invitation.id;
      const title = document.createElement('h2');
      title.textContent = invitation.name;
      const detail = document.createElement('p');
      const people = invitation.people || [];
      people.forEach((person) => {
        counts[person.status] = (counts[person.status] || 0) + 1;
      });
      detail.textContent = [invitation.groupName, `${people.length} pessoa(s)`, invitation.whatsApp].filter(Boolean).join(' · ');

      const actions = document.createElement('div');
      actions.className = 'guest-actions';
      const peopleSection = document.createElement('div');
      peopleSection.id = `invitation-people-${invitation.id}`;
      peopleSection.hidden = true;
      const peopleToggle = document.createElement('button');
      peopleToggle.type = 'button';
      peopleToggle.textContent = `Mostrar convidados (${people.length})`;
      peopleToggle.setAttribute('aria-expanded', 'false');
      peopleToggle.setAttribute('aria-controls', peopleSection.id);
      peopleToggle.addEventListener('click', () => {
        const expanded = peopleToggle.getAttribute('aria-expanded') === 'true';
        peopleToggle.setAttribute('aria-expanded', String(!expanded));
        peopleSection.hidden = expanded;
        peopleToggle.textContent = `${expanded ? 'Mostrar' : 'Ocultar'} convidados (${people.length})`;
      });
      const editButton = document.createElement('button');
      editButton.type = 'button';
      editButton.textContent = 'Editar convite';
      editButton.addEventListener('click', () => {
        const form = row.querySelector('.guest-edit-form');
        form.hidden = !form.hidden;
      });
      const deleteButton = document.createElement('button');
      deleteButton.type = 'button';
      deleteButton.className = 'danger';
      deleteButton.textContent = 'Excluir convite';
      deleteButton.addEventListener('click', async () => {
        if (!window.confirm(`Excluir o convite de ${invitation.name} e todas as respostas associadas?`)) return;
        try {
          const deleteResponse = await adminFetch(`/api/admin/guests/${invitation.id}`, { method: 'DELETE' });
          if (!deleteResponse.ok) throw new Error(await readApiError(deleteResponse));
          await loadGuests();
          actionMessage.textContent = 'Convite excluído.';
        } catch (error) {
          actionMessage.textContent = error.message;
        }
      });
      actions.append(peopleToggle, editButton, deleteButton);

      const editForm = document.createElement('form');
      editForm.className = 'guest-edit-form';
      editForm.hidden = true;
      const fields = document.createElement('div');
      fields.className = 'guest-edit-fields';
      const nameInput = document.createElement('input');
      nameInput.name = 'name';
      nameInput.maxLength = 120;
      nameInput.required = true;
      nameInput.value = invitation.name;
      const groupInput = document.createElement('input');
      groupInput.name = 'groupName';
      groupInput.maxLength = 120;
      groupInput.value = invitation.groupName || '';
      const whatsappInput = document.createElement('input');
      whatsappInput.name = 'whatsApp';
      whatsappInput.maxLength = 30;
      whatsappInput.inputMode = 'tel';
      whatsappInput.value = invitation.whatsApp || '';
      const limitInput = document.createElement('input');
      limitInput.name = 'freeCompanionLimit';
      limitInput.type = 'number';
      limitInput.min = '0';
      limitInput.max = '5';
      limitInput.value = invitation.freeCompanionLimit;
      const freeLabel = document.createElement('label');
      const freeInput = document.createElement('input');
      freeInput.name = 'allowFreeCompanions';
      freeInput.type = 'checkbox';
      freeInput.checked = invitation.allowFreeCompanions;
      freeLabel.append(freeInput, document.createTextNode(' Permitir acompanhantes livres'));

      [
        ['Titular', nameInput],
        ['Grupo', groupInput],
        ['WhatsApp', whatsappInput],
        ['Limite livre', limitInput]
      ].forEach(([labelText, input]) => {
        const label = document.createElement('label');
        label.append(document.createTextNode(labelText), input);
        fields.append(label);
      });
      fields.append(freeLabel);
      const saveEdit = document.createElement('button');
      saveEdit.type = 'submit';
      saveEdit.textContent = 'Salvar edição';
      editForm.append(fields, saveEdit);
      editForm.addEventListener('submit', async (event) => {
        event.preventDefault();
        const allowFreeCompanions = freeInput.checked;
        try {
          const updateResponse = await adminFetch(`/api/admin/guests/${invitation.id}`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
              name: nameInput.value.trim(),
              groupName: groupInput.value.trim(),
              whatsApp: whatsappInput.value.trim(),
              allowFreeCompanions,
              freeCompanionLimit: allowFreeCompanions ? Number(limitInput.value) : 0
            })
          });
          if (!updateResponse.ok) throw new Error(await readApiError(updateResponse));
          await loadGuests();
          actionMessage.textContent = 'Convite atualizado.';
        } catch (error) {
          actionMessage.textContent = error.message;
        }
      });

      people.forEach((person) => {
        const personRow = document.createElement('div');
        personRow.className = 'person-row';
        const personName = document.createElement('span');
        personName.textContent = `${person.name}${person.isPrimary ? ' (titular)' : person.isFreeCompanion ? ' (livre)' : ' (fixo)'}`;
        const statusForm = document.createElement('form');
        statusForm.className = 'person-status-form';
        const statusSelect = document.createElement('select');
        [['a-confirmar', 'A confirmar'], ['vou', 'Vou'], ['nao-vou', 'Não vou']].forEach(([value, label]) => {
          const option = document.createElement('option');
          option.value = value;
          option.textContent = label;
          option.selected = person.status === value;
          statusSelect.append(option);
        });
        const statusButton = document.createElement('button');
        statusButton.type = 'submit';
        statusButton.textContent = 'Atualizar';
        statusForm.append(statusSelect, statusButton);
        statusForm.addEventListener('submit', async (event) => {
          event.preventDefault();
          try {
            const statusResponse = await adminFetch(`/api/admin/guests/${invitation.id}/people/${person.id}/status`, {
              method: 'PATCH',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify({ status: statusSelect.value })
            });
            if (!statusResponse.ok) throw new Error(await readApiError(statusResponse));
            await loadGuests();
            actionMessage.textContent = `Presença de ${person.name} atualizada.`;
          } catch (error) {
            actionMessage.textContent = error.message;
          }
        });
        if (!person.isPrimary && !person.isFreeCompanion) {
          const companionActions = document.createElement('div');
          companionActions.className = 'fixed-companion-edit';
          const nameInput = document.createElement('input');
          nameInput.type = 'text';
          nameInput.maxLength = 120;
          nameInput.value = person.name;
          nameInput.setAttribute('aria-label', `Nome de ${person.name}`);
          const renameButton = document.createElement('button');
          renameButton.type = 'button';
          renameButton.textContent = 'Renomear';
          renameButton.addEventListener('click', async () => {
            try {
              const renameResponse = await adminFetch(`/api/admin/guests/${invitation.id}/fixed-companions/${person.id}`, {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ name: nameInput.value.trim() })
              });
              if (!renameResponse.ok) throw new Error(await readApiError(renameResponse));
              await loadGuests();
              actionMessage.textContent = 'Acompanhante fixo atualizado.';
            } catch (error) {
              actionMessage.textContent = error.message;
            }
          });

          const removeButton = document.createElement('button');
          removeButton.type = 'button';
          removeButton.className = 'danger';
          removeButton.textContent = 'Remover';
          removeButton.addEventListener('click', async () => {
            if (!window.confirm(`Remover ${person.name} deste convite?`)) return;
            try {
              const removeResponse = await adminFetch(`/api/admin/guests/${invitation.id}/fixed-companions/${person.id}`, { method: 'DELETE' });
              if (!removeResponse.ok) throw new Error(await readApiError(removeResponse));
              await loadGuests();
              actionMessage.textContent = 'Acompanhante fixo removido.';
            } catch (error) {
              actionMessage.textContent = error.message;
            }
          });
          companionActions.append(nameInput, renameButton, removeButton);
          personRow.append(personName, statusForm, companionActions);
        } else {
          personRow.append(personName, statusForm);
        }
        peopleSection.append(personRow);
      });

      const fixedCompanionForm = document.createElement('form');
      fixedCompanionForm.className = 'fixed-companion-form';
      const fixedCompanionInput = document.createElement('input');
      fixedCompanionInput.name = 'name';
      fixedCompanionInput.maxLength = 120;
      fixedCompanionInput.required = true;
      fixedCompanionInput.placeholder = 'Nome do acompanhante fixo';
      const addFixedCompanionButton = document.createElement('button');
      addFixedCompanionButton.type = 'submit';
      addFixedCompanionButton.textContent = 'Adicionar acompanhante fixo';
      fixedCompanionForm.append(fixedCompanionInput, addFixedCompanionButton);
      fixedCompanionForm.addEventListener('submit', async (event) => {
        event.preventDefault();
        try {
          const addResponse = await adminFetch(`/api/admin/guests/${invitation.id}/fixed-companions`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name: fixedCompanionInput.value.trim() })
          });
          if (!addResponse.ok) throw new Error(await readApiError(addResponse));
          await loadGuests();
          actionMessage.textContent = 'Acompanhante fixo cadastrado.';
        } catch (error) {
          actionMessage.textContent = error.message;
        }
      });
      peopleSection.append(fixedCompanionForm);

      row.append(title, detail, actions, editForm, peopleSection);
      guestList.append(row);
    });

    document.getElementById('adminGoing').textContent = counts.vou;
    document.getElementById('adminNotGoing').textContent = counts['nao-vou'];
    document.getElementById('adminPending').textContent = counts['a-confirmar'];
    createForm.hidden = false;
    giftAdminSection.hidden = false;
    await loadGifts();
    message.textContent = `${invitations.length} convite(s) carregado(s).`;
  };

  connectForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    adminKey = keyInput.value;
    message.textContent = 'Conectando...';
    try {
      await loadGuests();
    } catch (error) {
      adminKey = '';
      message.textContent = error.message;
      createForm.hidden = true;
      giftAdminSection.hidden = true;
    }
  });

  createForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    const form = new FormData(createForm);
    const allowFreeCompanions = form.get('allowFreeCompanions') === 'on';
    const request = {
      name: form.get('name').trim(),
      groupName: form.get('groupName').trim() || null,
      whatsApp: form.get('whatsApp').trim() || null,
      companions: form.get('companions').split(/\r?\n/).map((name) => name.trim()).filter(Boolean),
      allowFreeCompanions,
      freeCompanionLimit: allowFreeCompanions ? Number(form.get('freeCompanionLimit')) : 0
    };

    try {
      const response = await adminFetch('/api/admin/guests', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(request)
      });
      if (!response.ok) throw new Error(await readApiError(response));
      createForm.reset();
      createForm.elements.freeCompanionLimit.value = '0';
      await loadGuests();
      message.textContent = 'Convite cadastrado.';
    } catch (error) {
      message.textContent = error.message;
    }
  });

  giftCreateForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    const form = new FormData(giftCreateForm);
    const amountInCents = Math.round(Number(form.get('amount')) * 100);
    try {
      const response = await adminFetch('/api/admin/gifts', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name: form.get('name').trim(),
          description: form.get('description').trim(),
          amountInCents,
          imageUrl: form.get('imageUrl').trim() || null
        })
      });
      if (!response.ok) throw new Error(await readApiError(response));
      giftCreateForm.reset();
      await loadGifts();
      actionMessage.textContent = 'Presente simbólico cadastrado. Nenhuma cobrança foi criada.';
    } catch (error) {
      actionMessage.textContent = error.message;
    }
  });
}

