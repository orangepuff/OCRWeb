import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PortalHeader } from '../header/header';

@Component({
  selector: 'lib-portal-shell',
  imports: [RouterOutlet, PortalHeader],
  templateUrl: './portal-shell.html',
  styleUrl: './portal-shell.scss'
})
export class PortalShell {}
