import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-landing',
  imports: [],
  templateUrl: './landing.html',
  styleUrl: './landing.scss'
})
export class Landing implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.authService.checkSession().subscribe(() => {
      if (this.authService.isAuthenticated()) {
        this.router.navigateByUrl('/home');
      }
    });
  }

  protected signIn(): void {
    this.authService.login('/home');
  }
}
