// ═══════════════════════════════════════════════════════════
// Generación de Reportes PDF usando jsPDF
// ═══════════════════════════════════════════════════════════

window.GenerarReportePdf = (configuracion) => {
    try {
        const { jsPDF } = window.jspdf;

        // Crear documento
        const doc = new jsPDF({
            orientation: configuracion.orientacion || 'landscape',
            unit: 'mm',
            format: 'a4'
        });

        const pageWidth = doc.internal.pageSize.getWidth();
        const pageHeight = doc.internal.pageSize.getHeight();
        const colorPrimario = configuracion.colorPrimario || '#1976D2';

        // ═══════════════════════════════════════════
        // 1. ENCABEZADO DEL HOTEL
        // ═══════════════════════════════════════════
        doc.setFillColor(colorPrimario);
        doc.rect(0, 0, pageWidth, 20, 'F');

        doc.setFontSize(16);
        doc.setTextColor(255, 255, 255);
        doc.setFont('helvetica', 'bold');
        doc.text(configuracion.nombreHotel || 'HOSTERÍA AGOYÁN', pageWidth / 2, 10, { align: 'center' });

        doc.setFontSize(8);
        doc.setFont('helvetica', 'normal');
        const datosHotel = `RUC: ${configuracion.ruc} | ${configuracion.direccion} | Tel: ${configuracion.telefono}`;
        doc.text(datosHotel, pageWidth / 2, 16, { align: 'center' });

        // ═══════════════════════════════════════════
        // 2. TÍTULO DEL REPORTE
        // ═══════════════════════════════════════════
        doc.setTextColor(colorPrimario);
        doc.setFontSize(14);
        doc.setFont('helvetica', 'bold');
        doc.text(configuracion.titulo.toUpperCase(), pageWidth / 2, 30, { align: 'center' });

        // Subtítulo (filtros)
        if (configuracion.subtitulo) {
            doc.setTextColor(80, 80, 80);
            doc.setFontSize(10);
            doc.setFont('helvetica', 'italic');
            doc.text(configuracion.subtitulo, pageWidth / 2, 36, { align: 'center' });
        }

        // Metadata
        doc.setFontSize(8);
        doc.setFont('helvetica', 'normal');
        const metadata = `Generado por: ${configuracion.usuarioGenerador} | Fecha: ${configuracion.fechaGeneracion} | Total: ${configuracion.datos.length} registros`;
        doc.text(metadata, pageWidth / 2, 42, { align: 'center' });

        // ═══════════════════════════════════════════
        // 3. TABLA CON DATOS
        // ═══════════════════════════════════════════
        doc.autoTable({
            head: [configuracion.encabezados],
            body: configuracion.datos,
            startY: 47,
            theme: 'grid',
            headStyles: {
                fillColor: colorPrimario,
                textColor: [255, 255, 255],
                fontStyle: 'bold',
                halign: 'center',
                fontSize: 9
            },
            bodyStyles: {
                fontSize: 8,
                cellPadding: 2
            },
            alternateRowStyles: {
                fillColor: [245, 245, 245]
            },
            columnStyles: configuracion.estilosColumnas || {},
            didDrawPage: function (data) {
                // Pie de página
                const str = `Página ${doc.internal.getNumberOfPages()} | ${configuracion.nombreHotel || 'Hostería Agoyán'}`;
                doc.setFontSize(8);
                doc.setTextColor(150, 150, 150);
                doc.text(str, pageWidth / 2, pageHeight - 10, { align: 'center' });
            },
            margin: { top: 47 }
        });

        // ═══════════════════════════════════════════
        // 4. TOTALES (si aplica)
        // ═══════════════════════════════════════════
        if (configuracion.totales && configuracion.totales.length > 0) {
            const finalY = doc.lastAutoTable.finalY + 5;

            doc.setFillColor(220, 235, 250);
            doc.rect(10, finalY, pageWidth - 20, 8, 'F');

            doc.setTextColor(colorPrimario);
            doc.setFontSize(10);
            doc.setFont('helvetica', 'bold');

            let textoTotal = 'TOTALES:  ';
            configuracion.totales.forEach((t, i) => {
                textoTotal += `${t.label}: $${t.valor.toLocaleString('es-EC', { minimumFractionDigits: 2 })}    `;
            });

            doc.text(textoTotal.trim(), pageWidth / 2, finalY + 5, { align: 'center' });
        }

        // ═══════════════════════════════════════════
        // 5. DESCARGAR
        // ═══════════════════════════════════════════
        const fechaArchivo = new Date().toISOString().slice(0, 10).replace(/-/g, '');
        doc.save(`${configuracion.nombreArchivo}_${fechaArchivo}.pdf`);

        return true;
    } catch (error) {
        console.error('Error generando PDF:', error);
        alert('Error al generar el PDF: ' + error.message);
        return false;
    }
};
