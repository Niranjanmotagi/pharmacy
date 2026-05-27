import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

import { Entity, EntityService } from '../../services/entity.service';

@Component({
  selector: 'app-entity-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './entity-list.html',
  styleUrl: './entity-list.css'
})
export class EntityList implements OnInit {

  entities: Entity[] = [];

  constructor(private entityService: EntityService) {}

  ngOnInit(): void {
    this.loadEntities();
  }

  loadEntities(): void {
    this.entityService.getEntities().subscribe({
      next: (response: Entity[]) => {
        this.entities = response ?? [];
      },
      error: (err: unknown) => {
        console.error('[entity-list] failed to load entities', err);
      }
    });
  }
}
