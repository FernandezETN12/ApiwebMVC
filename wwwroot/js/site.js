document.addEventListener("DOMContentLoaded", () => {
	const toast = document.createElement("div");
	toast.className = "site-toast";
	toast.setAttribute("role", "status");
	toast.setAttribute("aria-live", "polite");
	Object.assign(toast.style, {
		position: "fixed",
		right: "1.25rem",
		top: "6rem",
		bottom: "auto",
		zIndex: "1080",
		maxWidth: "min(24rem, calc(100vw - 2.5rem))",
		padding: ".8rem 1rem",
		color: "#fff",
		background: "#211b18",
		borderLeft: ".25rem solid #b8121b",
		borderRadius: ".25rem",
		boxShadow: "0 8px 24px rgba(0, 0, 0, .2)",
		opacity: "0",
		transform: "translateY(.5rem)",
		transition: "opacity .18s ease, transform .18s ease",
		pointerEvents: "none"
	});
	document.body.append(toast);

	let toastTimeout;
	const showToast = (message, isError = false) => {
		toast.textContent = message;
		toast.style.borderLeftColor = isError ? "#dc3545" : "#b8121b";
		toast.style.opacity = "1";
		toast.style.transform = "translateY(0)";
		clearTimeout(toastTimeout);
		toastTimeout = setTimeout(() => {
			toast.style.opacity = "0";
			toast.style.transform = "translateY(.5rem)";
		}, 2600);
	};

	document.querySelectorAll(".js-auto-submit-quantity input[name='cantidad']")
		.forEach((input) => {
			input.addEventListener("change", () => input.form?.requestSubmit());
		});

	document.querySelectorAll(".js-add-to-cart").forEach((form) => {
		form.addEventListener("submit", async (event) => {
			event.preventDefault();
			const button = form.querySelector("button[type='submit']");
			if (!button || button.disabled) return;

			button.disabled = true;
			try {
				const response = await fetch(form.action, {
					method: "POST",
					body: new FormData(form),
					credentials: "same-origin",
					headers: { "X-Requested-With": "XMLHttpRequest" }
				});

				if (!response.ok) {
					throw new Error("No se pudo agregar el producto.");
				}

				const responseHtml = await response.text();
				const responseDocument = new DOMParser().parseFromString(responseHtml, "text/html");
				const updatedSummaryCount = responseDocument.querySelector("#cart-summary-count")?.textContent?.trim();
				const updatedSummaryTotal = responseDocument.querySelector("#cart-summary-total")?.textContent?.trim();

				const summaryCount = document.querySelector("#cart-summary-count");
				const summaryTotal = document.querySelector("#cart-summary-total");
				const cartSummary = document.querySelector(".cart-summary");

				if (summaryCount && updatedSummaryCount !== undefined) summaryCount.textContent = updatedSummaryCount;
				if (summaryTotal && updatedSummaryTotal !== undefined) summaryTotal.textContent = updatedSummaryTotal;
				if (cartSummary && updatedSummaryCount !== undefined) {
					cartSummary.hidden = Number(updatedSummaryCount) === 0;
				}

				showToast(`${form.dataset.productName} agregado al pedido.`);
			} catch {
				showToast("No se pudo agregar el producto. Intenta nuevamente.", true);
			} finally {
				button.disabled = false;
			}
		});
	});

	const heroPlate = document.querySelector(".brand-hero__visual .hero-plate");
	if (heroPlate) {
		const pizzaPhoto = document.createElement("img");
		pizzaPhoto.src = "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=1000&q=85";
		pizzaPhoto.alt = "Pizza artesanal recién horneada con queso dorado";
		pizzaPhoto.width = 1000;
		pizzaPhoto.height = 667;
		pizzaPhoto.loading = "eager";
		pizzaPhoto.decoding = "async";
		Object.assign(pizzaPhoto.style, {
			display: "block",
			width: "min(100%, 500px)",
			aspectRatio: "1",
			objectFit: "cover",
			objectPosition: "58% center",
			border: ".65rem solid #fff",
			borderRadius: "50%",
			boxShadow: "0 26px 58px rgba(55, 31, 26, .2)",
			transform: "rotate(-4deg)"
		});
		heroPlate.replaceWith(pizzaPhoto);
	}

	const pizzaModal = document.getElementById('pizzaModal');
	if (pizzaModal) {
		pizzaModal.addEventListener('show.bs.modal', (event) => {
			const card = event.relatedTarget;
			if (!card?.dataset) return;
			const ingredients = (card.dataset.pizzaIngredients || '').split('|').filter(Boolean);
			document.getElementById('pizzaModalTitle').textContent = card.dataset.pizzaName || '';
			document.getElementById('pizzaModalDescription').textContent = card.dataset.pizzaDescription || '';
			document.getElementById('pizzaModalImage').style.backgroundImage = `url('${card.dataset.pizzaImage || ''}')`;
			document.getElementById('pizzaModalIngredients').replaceChildren(
				...ingredients.map((ingredient) => {
					const tag = document.createElement('span');
					tag.textContent = ingredient;
					return tag;
				})
			);
		});
	}
});
