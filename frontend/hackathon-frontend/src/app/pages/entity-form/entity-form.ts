import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

import { Entity, EntityService } from '../../services/entity.service';

interface EntityFormModel {
  id: number;
  name: string;
  description: string;
}

@Component({
  selector: 'app-entity-form',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './entity-form.html',
  styleUrl: './entity-form.css'
})
export class EntityForm {

  entity: EntityFormModel = {
    id: 0,
    name: '',
    description: ''
  };

  constructor(
    private entityService: EntityService,
    private router: Router
  ) {}

  addEntity(): void {
    this.entityService.addEntity(this.entity).subscribe({
      next: (_response: Entity) => {
        alert('Entity Added');
        this.router.navigate(['/entities']);
      },
      error: (err: unknown) => {
        console.error('[entity-form] failed to add entity', err);
      }
    });
  }
}
