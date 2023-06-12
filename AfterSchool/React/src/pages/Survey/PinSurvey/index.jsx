import { useSnackbar } from 'notistack';
import React, { useEffect, useState } from 'react';
import { useParams, useSearchParams } from 'react-router-dom';

import { useSurvey } from 'redux/hooks';
import { PinModal } from 'components';
import PinSurvey from './PinSurvey';

function SurveyPinModal() {
  const { srUid } = useParams();
  const [pin, setPin] = useState();
  const [submitDisable, setSubmitDisable] = useState(false);
  const { enqueueSnackbar } = useSnackbar();
  const [searchParams, setSearchParams] = useSearchParams();

  const { msgs, loadFormData } = useSurvey();

  useEffect(() => {
    msgs.forEach(msg => enqueueSnackbar(msg.Text, { variant: 'error' }));
  }, [msgs]);

  const pinSubmitHandler = async () => {
    setSubmitDisable(true);
    await loadFormData(srUid, null, pin).then(() => {
      if (!msgs.length) setSearchParams(`pin=${pin}`);
      else setSubmitDisable(false);
    });
  };

  if (searchParams.get('pin')) return <PinSurvey />;

  return (
    <PinModal
      isOpen={true}
      submitDisabled={submitDisable}
      onChange={v => setPin(v.target.value)}
      onSubmit={pinSubmitHandler}
    />
  );
}

export default SurveyPinModal;
