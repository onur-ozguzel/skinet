import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { CartService } from '../services/cart.service';
import { SnackbarService } from '../services/snackbar.service';

export const emptyCartGuard: CanActivateFn = (route, state) => {
  const cartService = inject(CartService);
  const snack = inject(SnackbarService);
  const router = inject(Router);
  const itemCount = cartService.itemCount() ?? 0;

  if (itemCount > 0) {
    return true;
  } else {
    snack.error("Your cart is empty");
    router.navigateByUrl('/cart');
    return false;
  }
};
