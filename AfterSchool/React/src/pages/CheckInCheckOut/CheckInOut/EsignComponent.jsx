import React, { useRef } from 'react';
import SignatureCanvas from 'react-signature-canvas';
import TextField from 'components/Overrides/TextField';

export default function EsignComponent({ handleSubmit, type = 'pad', value = '' }) {
  const sigCanvas = useRef();

  const create = async () => {
    const URL = sigCanvas.current.getTrimmedCanvas().toDataURL('image/png');
    const imageFile = await fetch(URL)
      .then(res => res.blob())
      .then(blobFile => new File([blobFile], `signPhoto.jpg`, { type: 'image/png' }));
    handleSubmit(imageFile);
  };

  const handleChange = e => {
    handleSubmit(e.target.value);
  };

  return (
    <>
      {type === 'text' ? (
        <TextField value={value} onChange={handleChange} type="text" fullWidth />
      ) : (
        <SignatureCanvas
          penColor="black"
          backgroundColor="#B2B2B4"
          ref={sigCanvas}
          canvasProps={{ className: 'sigCanvas', height: 300 }}
          onEnd={() => create()}
        />
      )}
    </>
  );
}
